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
using slices = slices_package;
using sync = sync_package;
using @internal;
using go.math;

partial class big_package {

// An unsigned integer x of the form
//
//	x = x[n-1]*_B^(n-1) + x[n-2]*_B^(n-2) + ... + x[1]*_B + x[0]
//
// with 0 <= x[i] < _B and 0 <= i < n is stored in a slice of length n,
// with the digits x[i] as the slice elements.
//
// A number is normalized if the slice contains no leading 0 digits.
// During arithmetic operations, denormalized values may occur but are
// always normalized before returning the final result. The normalized
// representation of 0 is the empty or nil slice (length = 0).
partial struct Δnat /*[]Word*/;

internal static Δnat natOne = new Δnat(new Word[]{1}.slice());
internal static Δnat natTwo = new Δnat(new Word[]{2}.slice());
internal static Δnat natFive = new Δnat(new Word[]{5}.slice());
internal static Δnat natTen = new Δnat(new Word[]{10}.slice());

internal static @string String(this Δnat z) {
    return "0x"u8 + ((sstring)z.itoa(false, 16));
}

internal static Δnat norm(this Δnat z) {
    nint i = len(z);
    while (i > 0 && z[i - 1] == 0) {
        i--;
    }
    return z.slice(0, i);
}

internal static Δnat make(this Δnat z, nint n) {
    if (n <= cap(z)) {
        return z.slice(0, n); // reuse z
    }
    if (n == 1) {
        // Most nats start small and stay that way; don't over-allocate.
        return new Δnat(1);
    }
    // Choosing a good value for e has significant performance impact
    // because it increases the chance that a value can be reused.
    const nint e = 4; // extra capacity
    return new Δnat(n, n + e);
}

internal static Δnat setWord(this Δnat z, Word x) {
    if (x == 0) {
        return z[..0];
    }
    z = z.make(1);
    z[0] = x;
    return z;
}

internal static Δnat setUint64(this Δnat z, uint64 x) {
    // single-word value
    {
        Word w = (Word)(nuint)x; if ((uint64)w == x) {
            return z.setWord(w);
        }
    }
    // 2-word value
    z = z.make(2);
    z[1] = (Word)(nuint)((x >> 32));
    z[0] = (Word)(nuint)x;
    return z;
}

internal static Δnat set(this Δnat z, Δnat x) {
    z = z.make(len(x));
    copy(z, x);
    return z;
}

internal static Δnat add(this Δnat z, Δnat x, Δnat y) {
    nint m = len(x);
    nint n = len(y);

    switch (ᐧ) {
    case {} when m < n: {
        return z.add(y, x);
    }
    case {} when m is 0: {
        // n == 0 because m >= n; result is 0
        return z[..0];
    }
    case {} when n is 0: {
        // result is x
        return z.set(x);
    }}
    // m > 0

    z = z.make(m + 1);
    Word c = addVV(z.slice(0, n), x.slice(0, n), y.slice(0, n));
    if (m > n) {
        c = addVW(z.slice(n, m), x.slice(n), c);
    }
    z[m] = c;

    return z.norm();
}

internal static Δnat sub(this Δnat z, Δnat x, Δnat y) {
    nint m = len(x);
    nint n = len(y);

    switch (ᐧ) {
    case {} when m < n: {
        throw panic("underflow");
        break;
    }
    case {} when m is 0: {
        // n == 0 because m >= n; result is 0
        return z[..0];
    }
    case {} when n is 0: {
        // result is x
        return z.set(x);
    }}
    // m > 0

    z = z.make(m);
    Word c = subVV(z.slice(0, n), x.slice(0, n), y.slice(0, n));
    if (m > n) {
        c = subVW(z.slice(n), x.slice(n), c);
    }
    if (c != 0) {
        throw panic("underflow");
    }

    return z.norm();
}

internal static nint /*r*/ cmp(this Δnat x, Δnat y) {
    nint r = default;

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

// montgomery computes z mod m = x*y*2**(-n*_W) mod m,
// assuming k = -1/m mod 2**_W.
// z is used for storing the result which is returned;
// z must not alias x, y or m.
// See Gueron, "Efficient Software Implementations of Modular Exponentiation".
// https://eprint.iacr.org/2011/239.pdf
// In the terminology of that paper, this is an "Almost Montgomery Multiplication":
// x and y are required to satisfy 0 <= z < 2**(n*_W) and then the result
// z is guaranteed to satisfy 0 <= z < 2**(n*_W), but it may not be < m.
internal static Δnat montgomery(this Δnat z, Δnat x, Δnat y, Δnat m, Word k, nint n) {
    // This code assumes x, y, m are all the same length, n.
    // (required by addMulVVW and the for loop).
    // It also assumes that x, y are already reduced mod m,
    // or else the result will not be properly reduced.
    if (len(x) != n || len(y) != n || len(m) != n) {
        throw panic("math/big: mismatched montgomery number lengths");
    }
    z = z.make(n * 2);
    clear(z);
    Word c = default;
    for (nint i = 0; i < n; i++) {
        Word d = y[i];
        Word c2 = addMulVVWW(z.slice(i, n + i), z.slice(i, n + i), x, d, 0);
        Word t = z[i] * k;
        Word c3 = addMulVVWW(z.slice(i, n + i), z.slice(i, n + i), m, t, 0);
        Word cx = c + c2;
        Word cy = cx + c3;
        z[n + i] = cy;
        if (cx < c2 || cy < c3) {
            c = 1;
        } else {
            c = 0;
        }
    }
    if (c != 0) {
        subVV(z.slice(0, n), z.slice(n), m);
    } else {
        copy(z.slice(0, n), z.slice(n));
    }
    return z.slice(0, n);
}

// alias reports whether x and y share the same base array.
//
// Note: alias assumes that the capacity of underlying arrays
// is never changed for nat values; i.e. that there are
// no 3-operand slice expressions in this code (or worse,
// reflect-based operations to the same effect).
internal static bool alias(Δnat x, Δnat y) {
    return cap(x) > 0 && cap(y) > 0 && Ꮡelem<Word>(x.slice(0, cap(x)), cap(x) - 1) == Ꮡelem<Word>(y.slice(0, cap(y)), cap(y) - 1);
}

// addTo implements z += x; z must be long enough.
// (we don't use nat.add because we need z to stay the same
// slice, and we don't need to normalize z after each addition)
internal static void addTo(Δnat z, Δnat x) {
    {
        nint n = len(x); if (n > 0) {
            {
                Word c = addVV(z.slice(0, n), z.slice(0, n), x.slice(0, n)); if (c != 0) {
                    if (n < len(z)) {
                        addVW(z.slice(n), z.slice(n), c);
                    }
                }
            }
        }
    }
}

// mulRange computes the product of all the unsigned integers in the
// range [a, b] inclusively. If a > b (empty range), the result is 1.
// The caller may pass stk == nil to request that mulRange obtain and release one itself.
internal static Δnat mulRange(this Δnat z, ж<stack> Ꮡstk, uint64 a, uint64 b) {
    GoFrame ᒐ = default;
    try {
        ref var stk = ref Ꮡstk.DerefOrNull();

        switch (ᐧ) {
        case {} when a is 0: {
            // cut long ranges short (optimization)
            return z.setUint64(0);
        }
        case {} when a > b: {
            return z.setUint64(1);
        }
        case {} when a == b: {
            return z.setUint64(a);
        }
        case {} when a + 1 == b: {
            return z.mul(Ꮡstk, ((Δnat)default).setUint64(a), ((Δnat)default).setUint64(b));
        }}

        if (Ꮡstk == nil) {
            Ꮡstk = getStack(); stk = ref Ꮡstk.DerefOrNull();
            defer(Ꮡstk.free, ref ᒐ);
        }

        var m = a + (b - a) / 2; // avoid overflow
        return z.mul(Ꮡstk, ((Δnat)default).mulRange(Ꮡstk, a, m), ((Δnat)default).mulRange(Ꮡstk, m + 1, b));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// A stack provides temporary storage for complex calculations
// such as multiplication and division.
// The stack is a simple slice of words, extended as needed
// to hold all the temporary storage for a calculation.
// In general, if a function takes a *stack, it expects a non-nil *stack.
// However, certain functions may allow passing a nil *stack instead,
// so that they can handle trivial stack-free cases without forcing the
// caller to obtain and free a stack that will be unused. These functions
// document that they accept a nil *stack in their doc comments.
partial struct stack {
    internal slice<Word> w;
}

internal static readonly ж<sync.Pool> ᏑstackPool = new StaticBox<sync.Pool>(default(sync.Pool));
internal static ref sync.Pool stackPool => ref ((StaticBox<sync.Pool>)ᏑstackPool).ValueSlot;

// getStack returns a temporary stack.
// The caller must call [stack.free] to give up use of the stack when finished.
internal static ж<stack> getStack() {
    var (s, _) = ᏑstackPool.Get()._<ж<stack>>(ᐧ);
    if (s == nil) {
        s = @new<stack>();
    }
    return s;
}

// free returns the stack for use by another calculation.
internal static void free(this ж<stack> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    s.w = s.w[..0];
    ᏑstackPool.Put(Ꮡs.OrTypedNil());
}

// save returns the current stack pointer.
// A future call to restore with the same value
// frees any temporaries allocated on the stack after the call to save.
internal static nint save(this ref stack s) {
    return len(s.w);
}

// restore restores the stack pointer to n.
// It is almost always invoked as
//
//	defer stk.restore(stk.save())
//
// which makes sure to pop any temporaries allocated in the current function
// from the stack before returning.
internal static void restore(this ref stack s, nint n) {
    s.w = s.w.slice(0, n);
}

// nat returns a nat of n words, allocated on the stack.
internal static Δnat nat(this ref stack s, nint n) {
    nint nr = (n + 3) & ~(nint)3; // round up to multiple of 4
    nint off = len(s.w);
    s.w = slices.Grow<slice<Word>, Word>(s.w, nr);
    s.w = s.w.slice(0, off + nr);
    var x = s.w.slice(off, off + n, off + n);
    if (n > 0) {
        x[0] = 0xfedcb;
    }
    return x;
}

// bitLen returns the length of x in bits.
// Unlike most methods, it works even if x is not normalized.
internal static nint bitLen(this Δnat x) {
    // This function is used in cryptographic operations. It must not leak
    // anything but the Int's sign and bit size through side-channels. Any
    // changes must be reviewed by a security expert.
    {
        nint i = len(x) - 1; if (i >= 0) {
            // bits.Len uses a lookup table for the low-order bits on some
            // architectures. Neutralize any input-dependent behavior by setting all
            // bits after the first one bit.
            nuint top = (nuint)x[i];
            top |= (nuint)((top >> 1));
            top |= (nuint)((top >> 2));
            top |= (nuint)((top >> 4));
            top |= (nuint)((top >> 8));
            top |= (nuint)((top >> 16));
            top |= (nuint)(((top >> 16) >> 16)); // ">> 32" doesn't compile on 32-bit architectures
            return i * (nint)_W + bits.Len(top);
        }
    }
    return 0;
}

// trailingZeroBits returns the number of consecutive least significant zero
// bits of x.
internal static nuint trailingZeroBits(this Δnat x) {
    if (len(x) == 0) {
        return 0;
    }
    nuint i = default;
    while (x[i] == 0) {
        i++;
    }
    // x[i] != 0
    return i * (nuint)_W + (nuint)bits.TrailingZeros((nuint)x[i]);
}

// isPow2 returns i, true when x == 2**i and 0, false otherwise.
internal static (nuint, bool) isPow2(this Δnat x) {
    nuint i = default;
    while (x[i] == 0) {
        i++;
    }
    if (i == (nuint)len(x) - 1 && (Word)(x[i] & (x[i] - 1)) == 0) {
        return (i * (nuint)_W + (nuint)bits.TrailingZeros((nuint)x[i]), true);
    }
    return (0, false);
}

internal static bool same(Δnat x, Δnat y) {
    return len(x) == len(y) && len(x) > 0 && Ꮡelem<Word>(x, 0) == Ꮡelem<Word>(y, 0);
}

// z = x << s
internal static Δnat lsh(this Δnat z, Δnat x, nuint s) {
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
    {
        s %= _W; if (s == 0) {
            copy(z.slice(n - m, n), x);
            z[n] = 0;
        } else {
            z[n] = lshVU(z.slice(n - m, n), x, s);
        }
    }
    clear(z.slice(0, n - m));

    return z.norm();
}

// z = x >> s
internal static Δnat rsh(this Δnat z, Δnat x, nuint s) {
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
    {
        s %= _W; if (s == 0) {
            copy(z, x.slice(m - n));
        } else {
            rshVU(z, x.slice(m - n), s);
        }
    }

    return z.norm();
}

internal static Δnat setBit(this Δnat z, Δnat x, nuint i, nuint b) {
    nint j = (nint)(i / (nuint)_W);
    Word m = ((Word)1) << (int)(i % (nuint)_W);
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
        if (j >= n) {
            z = z.make(j + 1);
            clear(z.slice(n));
        } else {
            z = z.make(n);
        }
        copy(z, x);
        z[j] |= (Word)(m);
        // no need to normalize
        return z;
    }}
    throw panic("set bit is not 0 or 1");
}

// bit returns the value of the i'th bit, with lsb == bit 0.
internal static nuint bit(this Δnat x, nuint i) {
    nuint j = i / (nuint)_W;
    if (j >= (nuint)len(x)) {
        return 0;
    }
    // 0 <= j < len(x)
    return (nuint)((Word)(x[j] >> (int)(i % (nuint)_W) & 1));
}

// sticky returns 1 if there's a 1 bit within the
// i least significant bits, otherwise it returns 0.
internal static nuint sticky(this Δnat x, nuint i) {
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
    if (x[j] << ((nuint)_W - i % (nuint)_W) != 0) {
        return 1;
    }
    return 0;
}

internal static Δnat and(this Δnat z, Δnat x, Δnat y) {
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
internal static Δnat trunc(this Δnat z, Δnat x, nuint n) {
    nuint w = (n + (nuint)_W - 1) / (nuint)_W;
    if ((nuint)len(x) < w) {
        return z.set(x);
    }
    z = z.make((nint)w);
    copy(z, x);
    if (n % (nuint)_W != 0) {
        z[len(z) - 1] &= (Word)((Word)((nuint)1 << (int)(n % (nuint)_W)) - 1);
    }
    return z.norm();
}

internal static Δnat andNot(this Δnat z, Δnat x, Δnat y) {
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

internal static Δnat or(this Δnat z, Δnat x, Δnat y) {
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

internal static Δnat xor(this Δnat z, Δnat x, Δnat y) {
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
internal static Δnat random(this Δnat z, ж<rand.Rand> Ꮡrand, Δnat limit, nint n) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    if (alias(z, limit)) {
        z = default; // z is an alias for limit - cannot reuse
    }
    z = z.make(len(limit));

    nuint bitLengthOfMSW = (nuint)(n % (nint)_W);
    if (bitLengthOfMSW == 0) {
        bitLengthOfMSW = _W;
    }
    Word mask = ((Word)((nuint)1 << (int)(bitLengthOfMSW))) - 1;

    for (;;) {
        switch (_W) {
        case var ᴛ when ᴛ == 32: {
            foreach (var i in range(z)) {
                z[i] = (Word)(nuint)randΔ1.Uint32();
            }
            break;
        }
        case var ᴛ when ᴛ == 64: {
            foreach (var i in range(z)) {
                z[i] = (Word)(((Word)(nuint)randΔ1.Uint32()) | ((Word)(nuint)randΔ1.Uint32()) << 32);
            }
            break;
        }
        default: {
            throw panic("unknown word size");
            break;
        }}
        z[len(limit) - 1] &= (Word)(mask);
        if (z.cmp(limit) < 0) {
            break;
        }
    }

    return z.norm();
}

// If m != 0 (i.e., len(m) != 0), expNN sets z to x**y mod m;
// otherwise it sets z to x**y. The result is the value of z.
// The caller may pass stk == nil to request that expNN obtain and release one itself.
internal static Δnat expNN(this Δnat z, ж<stack> Ꮡstk, Δnat x, Δnat y, Δnat m, bool slow) {
    GoFrame ᒐ = default;
    try {
        ref var stk = ref Ꮡstk.DerefOrNull();

        if (alias(z, x) || alias(z, y)) {
            // We cannot allow in-place modification of x or y.
            z = default;
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
        if (len(y) == 1 && y[0] == 1 && len(m) == 0) {
            return z.set(x);
        }
        if (Ꮡstk == nil) {
            Ꮡstk = getStack(); stk = ref Ꮡstk.DerefOrNull();
            defer(Ꮡstk.free, ref ᒐ);
        }
        if (len(y) == 1 && y[0] == 1) { // len(m) > 0
            return z.rem(Ꮡstk, x, m);
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
                    return z.expNNMontgomery(Ꮡstk, x, y, m);
                }
                {
                    var (logM, ok) = m.isPow2(); if (ok) {
                        return z.expNNWindowed(Ꮡstk, x, y, logM);
                    }
                }
                return z.expNNMontgomeryEven(Ꮡstk, x, y, m);
            }
        }

        z = z.set(x);
        Word v = y[len(y) - 1]; // v > 0 because y is normalized and y > 0
        nuint shift = nlz(v) + 1;
        v <<= shift;
        Δnat q = default;

        UntypedInt mask = /* 1 << (_W - 1) */ 9223372036854775808;

        // We walk through the bits of the exponent one by one. Each time we
        // see a bit, we square, thus doubling the power. If the bit is a one,
        // we also multiply by x, thus adding one to the power.

        nint w = (nint)_W - (nint)shift;
        // zz and r are used to avoid allocating in mul and div as
        // otherwise the arguments would alias.
        Δnat zz = default;
        Δnat r = default;
        for (nint j = 0; j < w; j++) {
            zz = zz.sqr(Ꮡstk, z);
            (zz, z) = (z, zz);

            if ((Word)(v & (nuint)mask) != 0) {
                zz = zz.mul(Ꮡstk, z, x);
                (zz, z) = (z, zz);
            }

            if (len(m) != 0) {
                (zz, r) = zz.div(Ꮡstk, r, z, m);
                (zz, r, q, z) = (q, z, zz, r);
            }

            v <<= (int)(1);
        }

        for (nint i = len(y) - 2; i >= 0; i--) {
            v = y[i];

            for (nint j = 0; j < _W; j++) {
                zz = zz.sqr(Ꮡstk, z);
                (zz, z) = (z, zz);

                if ((Word)(v & (nuint)mask) != 0) {
                    zz = zz.mul(Ꮡstk, z, x);
                    (zz, z) = (z, zz);
                }

                if (len(m) != 0) {
                    (zz, r) = zz.div(Ꮡstk, r, z, m);
                    (zz, r, q, z) = (q, z, zz, r);
                }

                v <<= (int)(1);
            }
        }

        return z.norm();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// expNNMontgomeryEven calculates x**y mod m where m = m1 × m2 for m1 = 2ⁿ and m2 odd.
// It uses two recursive calls to expNN for x**y mod m1 and x**y mod m2
// and then uses the Chinese Remainder Theorem to combine the results.
// The recursive call using m1 will use expNNWindowed,
// while the recursive call using m2 will use expNNMontgomery.
// For more details, see Ç. K. Koç, “Montgomery Reduction with Even Modulus”,
// IEE Proceedings: Computers and Digital Techniques, 141(5) 314-316, September 1994.
// http://www.people.vcu.edu/~jwang3/CMSC691/j34monex.pdf
internal static Δnat expNNMontgomeryEven(this Δnat z, ж<stack> stk, Δnat x, Δnat y, Δnat m) {
    // Split m = m₁ × m₂ where m₁ = 2ⁿ
    nuint n = m.trailingZeroBits();
    var m1 = ((Δnat)default).lsh(natOne, n);
    var m2 = ((Δnat)default).rsh(m, n);

    // We want z = x**y mod m.
    // z₁ = x**y mod m1 = (x**y mod m) mod m1 = z mod m1
    // z₂ = x**y mod m2 = (x**y mod m) mod m2 = z mod m2
    // (We are using the math/big convention for names here,
    // where the computation is z = x**y mod m, so its parts are z1 and z2.
    // The paper is computing x = a**e mod n; it refers to these as x2 and z1.)
    var z1 = ((Δnat)default).expNN(stk, x, y, m1, false);
    var z2 = ((Δnat)default).expNN(stk, x, y, m2, false);

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
    var m2inv = ((Δnat)default).modInverse(m2, m1);
    z2 = z2.mul(stk, z1, m2inv);
    z2 = z2.trunc(z2, n);

    // Reuse z1 for p * m2.
    z = z.add(z, z1.mul(stk, z2, m2));

    return z;
}

// expNNWindowed calculates x**y mod m using a fixed, 4-bit window,
// where m = 2**logM.
internal static Δnat expNNWindowed(this Δnat z, ж<stack> Ꮡstk, Δnat x, Δnat y, nuint logM) {
    GoFrame ᒐ = default;
    try {
        ref var stk = ref Ꮡstk.DerefOrNull();

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
        defer(Ꮡstk.restore, Ꮡstk.Value.save(), ref ᒐ);
        nint w = (nint)((logM + (nuint)_W - 1) / (nuint)_W);
        var zz = stk.nat(w);

        UntypedInt n = 4;
        // powers[i] contains x^i.
        ref var powers = ref heap(new array<Δnat>(16), out var Ꮡpowers);
        foreach (var iΔ1 in range(powers)) {
            powers[iΔ1] = stk.nat(w);
        }
        powers[0] = powers[0].set(natOne);
        powers[1] = powers[1].trunc(x, logM);
        for (nint iΔ2 = 2; iΔ2 < (nint)(1 << (int)(n)); iΔ2 += 2) {
            var (p2, p, p1) = (Ꮡpowers.at<Δnat>(iΔ2 / 2), Ꮡpowers.at<Δnat>(iΔ2), Ꮡpowers.at<Δnat>(iΔ2 + 1));
            p.ValueSlot = (~p).sqr(Ꮡstk, p2.ValueSlot);
            p.ValueSlot = (~p).trunc(p.ValueSlot, logM);
            p1.ValueSlot = (~p1).mul(Ꮡstk, p.ValueSlot, x);
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
            nuint mbits = (logM - 1) & (nuint)(_W - 1); if (mbits != 0) {
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
                    zz = zz.sqr(Ꮡstk, z);
                    (zz, z) = (z, zz);
                    z = z.trunc(z, logM);

                    zz = zz.sqr(Ꮡstk, z);
                    (zz, z) = (z, zz);
                    z = z.trunc(z, logM);

                    zz = zz.sqr(Ꮡstk, z);
                    (zz, z) = (z, zz);
                    z = z.trunc(z, logM);

                    zz = zz.sqr(Ꮡstk, z);
                    (zz, z) = (z, zz);
                    z = z.trunc(z, logM);
                }

                zz = zz.mul(Ꮡstk, z, powers[(yi >> (int)(_W - n))]);
                (zz, z) = (z, zz);
                z = z.trunc(z, logM);

                yi <<= (int)(n);
                advance = true;
            }
        }

        return z.norm();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// expNNMontgomery calculates x**y mod m using a fixed, 4-bit window.
// Uses Montgomery representation.
internal static Δnat expNNMontgomery(this Δnat z, ж<stack> stk, Δnat x, Δnat y, Δnat m) {
    nint numWords = len(m);

    // We want the lengths of x and m to be equal.
    // It is OK if x >= m as long as len(x) == len(m).
    if (len(x) > numWords) {
        (_, x) = ((Δnat)default).div(stk, default, x, m);
        // Note: now len(x) <= numWords, not guaranteed ==.
    }
    if (len(x) < numWords) {
        var rr = new Δnat(numWords);
        copy(rr, x);
        x = rr;
    }

    // Ideally the precomputations would be performed outside, and reused
    // k0 = -m**-1 mod 2**_W. Algorithm from: Dumas, J.G. "On Newton–Raphson
    // Iteration for Multiplicative Inverses Modulo Prime Powers".
    Word k0 = 2 - m[0];
    Word t = m[0] - 1;
    for (nint i = 1; i < _W; i <<= 1) {
        t *= t;
        k0 *= (t + 1);
    }
    k0 = ((Word)0 - k0);

    // RR = 2**(2*_W*len(m)) mod m
    var RR = ((Δnat)default).setWord(1);
    var zz = ((Δnat)default).lsh(RR, (nuint)(2 * numWords * (nint)_W));
    (_, RR) = ((Δnat)default).div(stk, RR, zz, m);
    if (len(RR) < numWords) {
        zz = zz.make(numWords);
        copy(zz, RR);
        RR = zz;
    }
    // one = 1, with equal length to that of m
    var one = new Δnat(numWords);
    one[0] = 1;

    UntypedInt n = 4;
    // powers[i] contains x^i
    array<Δnat> powers = new(16); /* (1 << (int)(n)) */
    powers[0] = powers[0].montgomery(one, RR, m, k0, numWords);
    powers[1] = powers[1].montgomery(x, RR, m, k0, numWords);
    for (nint i = 2; i < (nint)(1 << (int)(n)); i++) {
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
            zz = zz.montgomery(z, powers[(yi >> (int)(_W - n))], m, k0, numWords);
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
            (_, zz) = ((Δnat)default).div(stk, default, zz, m);
        }
    }

    return zz.norm();
}

// bytes writes the value of z into buf using big-endian encoding.
// The value of z is encoded in the slice buf[i:]. If the value of z
// cannot be represented in buf, bytes panics. The number i of unused
// bytes at the beginning of buf is returned as result.
internal static nint /*i*/ bytes(this Δnat z, slice<byte> buf) {
    nint i = default;

    // This function is used in cryptographic operations. It must not leak
    // anything but the Int's sign and bit size through side-channels. Any
    // changes must be reviewed by a security expert.
    i = len(buf);
    foreach (var (_, vᴛ1) in z) {
        var d = vᴛ1;

        for (nint j = 0; j < _S; j++) {
            i--;
            if (i >= 0) {
                buf[i] = (byte)d;
            } else if ((byte)d != 0) {
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
        return (Word)(nuint)byteorder.BEUint64(buf);
    }
    return (Word)(nuint)byteorder.BEUint32(buf);
}

// setBytes interprets buf as the bytes of a big-endian unsigned
// integer, sets z to that value, and returns z.
internal static Δnat setBytes(this Δnat z, slice<byte> buf) {
    z = z.make((len(buf) + (nint)_S - 1) / (nint)_S);

    nint i = len(buf);
    for (nint k = 0; i >= _S; k++) {
        z[k] = bigEndianWord(buf.slice(i - (nint)_S, i));
        i -= _S;
    }
    if (i > 0) {
        Word d = default;
        for (nuint s = (nuint)0; i > 0; s += 8) {
            d |= (Word)((((Word)(nuint)buf[i - 1]) << s));
            i--;
        }
        z[len(z) - 1] = d;
    }

    return z.norm();
}

// sqrt sets z = ⌊√x⌋
// The caller may pass stk == nil to request that sqrt obtain and release one itself.
internal static Δnat sqrt(this Δnat z, ж<stack> Ꮡstk, Δnat x) {
    GoFrame ᒐ = default;
    try {
        ref var stk = ref Ꮡstk.DerefOrNull();

        if (x.cmp(natOne) <= 0) {
            return z.set(x);
        }
        if (alias(z, x)) {
            z = default;
        }

        if (Ꮡstk == nil) {
            Ꮡstk = getStack(); stk = ref Ꮡstk.DerefOrNull();
            defer(Ꮡstk.free, ref ᒐ);
        }

        // Start with value known to be too large and repeat "z = ⌊(z + ⌊x/z⌋)/2⌋" until it stops getting smaller.
        // See Brent and Zimmermann, Modern Computer Arithmetic, Algorithm 1.13 (SqrtInt).
        // https://members.loria.fr/PZimmermann/mca/pub226.html
        // If x is one less than a perfect square, the sequence oscillates between the correct z and z+1;
        // otherwise it converges to the correct z and stays there.
        Δnat z1 = default;
        Δnat z2 = default;
        z1 = z;
        z1 = z1.setUint64(1);
        z1 = z1.lsh(z1, (nuint)(x.bitLen() + 1) / 2); // must be ≥ √x
        for (nint n = 0; ; n++) {
            (z2, _) = z2.div(Ꮡstk, default, x, z1);
            z2 = z2.add(z2, z1);
            z2 = z2.rsh(z2, 1);
            if (z2.cmp(z1) >= 0) {
                // z1 is answer.
                // Figure out whether z1 or z2 is currently aliased to z by looking at loop count.
                if ((n & 1) == 0) {
                    return z1;
                }
                return z.set(z1);
            }
            (z1, z2) = (z2, z1);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// subMod2N returns z = (x - y) mod 2ⁿ.
internal static Δnat subMod2N(this Δnat z, Δnat x, Δnat y, nuint n) {
    if ((nuint)x.bitLen() > n) {
        if (alias(z, x)) {
            // ok to overwrite x in place
            x = x.trunc(x, n);
        } else {
            x = ((Δnat)default).trunc(x, n);
        }
    }
    if ((nuint)y.bitLen() > n) {
        if (alias(z, y)) {
            // ok to overwrite y in place
            y = y.trunc(y, n);
        } else {
            y = ((Δnat)default).trunc(y, n);
        }
    }
    if (x.cmp(y) >= 0) {
        return z.sub(x, y);
    }
    // x - y < 0; x - y mod 2ⁿ = x - y + 2ⁿ = 2ⁿ - (y - x) = 1 + 2ⁿ-1 - (y - x) = 1 + ^(y - x).
    z = z.sub(y, x);
    while ((nuint)len(z) * (nuint)_W < n) {
        z = append(z, (Word)0);
    }
    foreach (var i in range(z)) {
        z[i] = ~z[i];
    }
    z = z.trunc(z, n);
    return z.add(z, natOne);
}

} // end big_package
