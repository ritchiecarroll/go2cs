// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements nat-to-string conversion functions.
namespace go.math;

using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using math = math_package;
using bits = go.math.bits_package;
using sync = sync_package;
using go.math;

partial class big_package {

internal static readonly @string digits = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"u8;

// Note: MaxBase = len(digits), but it must remain an untyped rune constant
//       for API compatibility.

// MaxBase is the largest number base accepted for string conversions.
public static UntypedInt MaxBase => /* 10 + ('z' - 'a' + 1) + ('Z' - 'A' + 1) */ 62;

internal static UntypedInt maxBaseSmall => /* 10 + ('z' - 'a' + 1) */ 36;

// maxPow returns (b**n, n) such that b**n is the largest power b**n <= _M.
// For instance maxPow(10) == (1e19, 19) for 19 decimal digits in a 64bit Word.
// In other words, at most n digits in base b fit into a Word.
// TODO(gri) replace this with a table, generated at build time.
internal static (Word p, nint n) maxPow(Word b) {
    Word p = default!;
    nint n = default!;

    (p, n) = (b, 1); // assuming b <= _M
    for (Word max = (nuint)_M / b; p <= max; ) {
        // p == b**n && p <= max
        p *= b;
        n++;
    }
    // p == b**n && p <= _M
    return (p, n);
}

// pow returns x**n for n > 0, and 1 otherwise.
internal static Word /*p*/ pow(Word x, nint n) {
    Word p = default!;

    // n == sum of bi * 2**i, for 0 <= i < imax, and bi is 0 or 1
    // thus x**n == product of x**(2**i) for all i where bi == 1
    // (Russian Peasant Method for exponentiation)
    p = 1;
    while (n > 0) {
        if ((nint)(n & 1) != 0) {
            p *= x;
        }
        x *= x;
        n >>= (int)(1);
    }
    return p;
}

// scan errors
internal static error errNoDigits = errors.New("number has no digits"u8);

internal static error errInvalSep = errors.New("'_' must separate successive digits"u8);

// scan scans the number corresponding to the longest possible prefix
// from r representing an unsigned number in a given conversion base.
// scan returns the corresponding natural number res, the actual base b,
// a digit count, and a read or syntax error err, if any.
//
// For base 0, an underscore character “_” may appear between a base
// prefix and an adjacent digit, and between successive digits; such
// underscores do not change the value of the number, or the returned
// digit count. Incorrect placement of underscores is reported as an
// error if there are no other errors. If base != 0, underscores are
// not recognized and thus terminate scanning like any other character
// that is not a valid radix point or digit.
//
//	number    = mantissa | prefix pmantissa .
//	prefix    = "0" [ "b" | "B" | "o" | "O" | "x" | "X" ] .
//	mantissa  = digits "." [ digits ] | digits | "." digits .
//	pmantissa = [ "_" ] digits "." [ digits ] | [ "_" ] digits | "." digits .
//	digits    = digit { [ "_" ] digit } .
//	digit     = "0" ... "9" | "a" ... "z" | "A" ... "Z" .
//
// Unless fracOk is set, the base argument must be 0 or a value between
// 2 and MaxBase. If fracOk is set, the base argument must be one of
// 0, 2, 8, 10, or 16. Providing an invalid base argument leads to a run-
// time panic.
//
// For base 0, the number prefix determines the actual base: A prefix of
// “0b” or “0B” selects base 2, “0o” or “0O” selects base 8, and
// “0x” or “0X” selects base 16. If fracOk is false, a “0” prefix
// (immediately followed by digits) selects base 8 as well. Otherwise,
// the selected base is 10 and no prefix is accepted.
//
// If fracOk is set, a period followed by a fractional part is permitted.
// The result value is computed as if there were no period present; and
// the count value is used to determine the fractional part.
//
// For bases <= 36, lower and upper case letters are considered the same:
// The letters 'a' to 'z' and 'A' to 'Z' represent digit values 10 to 35.
// For bases > 36, the upper case letters 'A' to 'Z' represent the digit
// values 36 to 61.
//
// A result digit count > 0 corresponds to the number of (non-prefix) digits
// parsed. A digit count <= 0 indicates the presence of a period (if fracOk
// is set, only), and -count is the number of fractional digits found.
// In this case, the actual value of the scanned number is res * b**count.
internal static (nat res, nint b, nint count, error err) scan(this nat z, io.ByteScanner r, nint @base, bool fracOk) {
    nat res = default!;
    nint b = default!;
    nint count = default!;
    error err = default!;

    // reject invalid bases
    var baseOk = @base == 0 || !fracOk && 2 <= @base && @base <= MaxBase || fracOk && (@base == 2 || @base == 8 || @base == 10 || @base == 16);
    if (!baseOk) {
        throw panic(fmt.Sprintf("invalid number base %d"u8, @base));
    }
    // prev encodes the previously seen char: it is one
    // of '_', '0' (a digit), or '.' (anything else). A
    // valid separator '_' may only occur after a digit
    // and if base == 0.
    var prev = (rune)'.';
    var invalSep = false;
    // one char look-ahead
    (var ch, err) = r.ReadByte();
    // determine actual base
    b = @base;
    nint prefix = 0;
    if (@base == 0) {
        // actual base is 10 unless there's a base prefix
        b = 10;
        if (err == default! && ch == (rune)'0') {
            prev = (rune)'0';
            count = 1;
            (ch, err) = r.ReadByte();
            if (err == default!) {
                // possibly one of 0b, 0B, 0o, 0O, 0x, 0X
                switch (ch) {
                case (rune)'b' or (rune)'B': {
                    (b, prefix) = (2, (rune)'b');
                    break;
                }
                case (rune)'o' or (rune)'O': {
                    (b, prefix) = (8, (rune)'o');
                    break;
                }
                case (rune)'x' or (rune)'X': {
                    (b, prefix) = (16, (rune)'x');
                    break;
                }
                default: {
                    if (!fracOk) {
                        (b, prefix) = (8, (rune)'0');
                    }
                    break;
                }}

                if (prefix != 0) {
                    count = 0; // prefix is not counted
                    if (prefix != (rune)'0') {
                        (ch, err) = r.ReadByte();
                    }
                }
            }
        }
    }
    // convert string
    // Algorithm: Collect digits in groups of at most n digits in di
    // and then use mulAddWW for every such group to add them to the
    // result.
    z = z[..0];
    Word b1 = ((Word)(nuint)b);
    var (bn, n) = maxPow(b1); // at most n digits in base b1 fit into Word
    Word di = ((Word)0); // 0 <= di < b1**i < bn
    nint i = 0; // 0 <= i < n
    nint dp = -1; // position of decimal point
    while (err == default!) {
        if (ch == (rune)'.' && fracOk){
            fracOk = false;
            if (prev == (rune)'_') {
                invalSep = true;
            }
            prev = (rune)'.';
            dp = count;
        } else 
        if (ch == (rune)'_' && @base == 0){
            if (prev != (rune)'0') {
                invalSep = true;
            }
            prev = (rune)'_';
        } else {
            // convert rune into digit value d1
            Word d1 = default!;
            switch (ᐧ) {
            case {} when (rune)'0' <= ch && ch <= (rune)'9': {
                d1 = ((Word)(nuint)((byte)(ch - (rune)'0')));
                break;
            }
            case {} when (rune)'a' <= ch && ch <= (rune)'z': {
                d1 = ((Word)(nuint)((byte)(ch - (rune)'a' + 10)));
                break;
            }
            case {} when (rune)'A' <= ch && ch <= (rune)'Z': {
                if (b <= maxBaseSmall){
                    d1 = ((Word)(nuint)((byte)(ch - (rune)'A' + 10)));
                } else {
                    d1 = ((Word)(nuint)((byte)(ch - (rune)'A' + (byte)maxBaseSmall)));
                }
                break;
            }
            default: {
                d1 = MaxBase + 1;
                break;
            }}

            if (d1 >= b1) {
                r.UnreadByte(); // ch does not belong to number anymore
                break;
            }
            prev = (rune)'0';
            count++;
            // collect d1 in di
            di = di * b1 + d1;
            i++;
            // if di is "full", add it to the result
            if (i == n) {
                z = z.mulAddWW(z, bn, di);
                di = 0;
                i = 0;
            }
        }
        (ch, err) = r.ReadByte();
    }
    if (AreEqual(err, io.EOF)) {
        err = default!;
    }
    // other errors take precedence over invalid separators
    if (err == default! && (invalSep || prev == (rune)'_')) {
        err = errInvalSep;
    }
    if (count == 0) {
        // no digits found
        if (prefix == (rune)'0') {
            // there was only the octal prefix 0 (possibly followed by separators and digits > 7);
            // interpret as decimal 0
            return (z[..0], 10, 1, err);
        }
        err = errNoDigits; // fall through; result will be 0
    }
    // add remaining digits to result
    if (i > 0) {
        z = z.mulAddWW(z, pow(b1, i), di);
    }
    res = z.norm();
    // adjust count for fraction, if any
    if (dp >= 0) {
        // 0 <= dp <= count
        count = dp - count;
    }
    return (res, b, count, err);
}

// utoa converts x to an ASCII representation in the given base;
// base must be between 2 and MaxBase, inclusive.
internal static slice<byte> utoa(this nat x, nint @base) {
    return x.itoa(false, @base);
}

// itoa is like utoa but it prepends a '-' if neg && x != 0.
internal static slice<byte> itoa(this nat x, bool neg, nint @base) {
    if (@base < 2 || @base > MaxBase) {
        throw panic("invalid base");
    }
    // x == 0
    if (len(x) == 0) {
        return slice<byte>("0"u8);
    }
    // len(x) > 0
    // allocate buffer for conversion
    nint i = (nint)((float64)x.bitLen() / math.Log2((float64)@base)) + 1; // off by 1 at most
    if (neg) {
        i++;
    }
    var s = new slice<byte>(i);
    // convert power of two and non power of two bases separately
    {
        Word b = ((Word)(nuint)@base); if (b == (Word)(b & ((Word)0 - b))){
            // shift is base b digit size in bits
            nuint shift = (nuint)bits.TrailingZeros((nuint)b); // shift > 0 because b >= 2
            Word mask = (Word)((nuint)1 << (int)(shift)) - 1;
            Word w = x[0]; // current word
            nuint nbits = (nuint)_W; // number of unprocessed bits in w
            // convert less-significant words (include leading zeros)
            for (nint k = 1; k < len(x); k++) {
                // convert full digits
                while (nbits >= shift) {
                    i--;
                    s[i] = digits[(Word)(w & mask)];
                    w >>= (int)(shift);
                    nbits -= shift;
                }
                // convert any partial leading digit and advance to next word
                if (nbits == 0){
                    // no partial digit remaining, just advance
                    w = x[k];
                    nbits = _W;
                } else {
                    // partial digit in current word w (== x[k-1]) and next word x[k]
                    w |= (Word)((x[k] << (int)(nbits)));
                    i--;
                    s[i] = digits[(Word)(w & mask)];
                    // advance
                    w = (x[k] >> (int)((shift - nbits)));
                    nbits = (nuint)_W - (shift - nbits);
                }
            }
            // convert digits of most-significant word w (omit leading zeros)
            while (w != 0) {
                i--;
                s[i] = digits[(Word)(w & mask)];
                w >>= (int)(shift);
            }
        } else {
            var (bb, ndigits) = maxPow(b);
            // construct table of successive squares of bb*leafSize to use in subdivisions
            // result (table != nil) <=> (len(x) > leafSize > 0)
            var table = divisors(len(x), b, ndigits, bb);
            // preserve x, create local copy for use by convertWords
            var q = ((nat)default!).set(x);
            // convert q to string s in base b
            q.convertWords(s, b, ndigits, bb, table);
            // strip leading zeros
            // (x != 0; thus s must contain at least one non-zero digit
            // and the loop will terminate)
            i = 0;
            while (s[i] == (rune)'0') {
                i++;
            }
        }
    }
    if (neg) {
        i--;
        s[i] = (rune)'-';
    }
    return s.slice(i);
}

// Convert words of q to base b digits in s. If q is large, it is recursively "split in half"
// by nat/nat division using tabulated divisors. Otherwise, it is converted iteratively using
// repeated nat/Word division.
//
// The iterative method processes n Words by n divW() calls, each of which visits every Word in the
// incrementally shortened q for a total of n + (n-1) + (n-2) ... + 2 + 1, or n(n+1)/2 divW()'s.
// Recursive conversion divides q by its approximate square root, yielding two parts, each half
// the size of q. Using the iterative method on both halves means 2 * (n/2)(n/2 + 1)/2 divW()'s
// plus the expensive long div(). Asymptotically, the ratio is favorable at 1/2 the divW()'s, and
// is made better by splitting the subblocks recursively. Best is to split blocks until one more
// split would take longer (because of the nat/nat div()) than the twice as many divW()'s of the
// iterative approach. This threshold is represented by leafSize. Benchmarking of leafSize in the
// range 2..64 shows that values of 8 and 16 work well, with a 4x speedup at medium lengths and
// ~30x for 20000 digits. Use nat_test.go's BenchmarkLeafSize tests to optimize leafSize for
// specific hardware.
internal static void convertWords(this nat q, slice<byte> s, Word b, nint ndigits, Word bb, slice<divisor> table) {
    // split larger blocks recursively
    if (table != default!) {
        // len(q) > leafSize > 0
        nat rΔ1 = default!;
        nint index = len(table) - 1;
        while (len(q) > leafSize) {
            // find divisor close to sqrt(q) if possible, but in any case < q
            nint maxLength = q.bitLen(); // ~= log2 q, or at of least largest possible q of this bit length
            nint minLength = (maxLength >> (int)(1)); // ~= log2 sqrt(q)
            while (index > 0 && table[index - 1].nbits > minLength) {
                index--; // desired
            }
            if (table[index].nbits >= maxLength && table[index].bbb.cmp(q) >= 0) {
                index--;
                if (index < 0) {
                    throw panic("internal inconsistency");
                }
            }
            // split q into the two digit number (q'*bbb + r) to form independent subblocks
            (q, rΔ1) = q.div(rΔ1, q, table[index].bbb);
            // convert subblocks and collect results in s[:h] and s[h:]
            nint h = len(s) - table[index].ndigits;
            rΔ1.convertWords(s.slice(h), b, ndigits, bb, table.slice(0, index));
            s = s.slice(0, h); // == q.convertWords(s, b, ndigits, bb, table[0:index+1])
        }
    }
    // having split any large blocks now process the remaining (small) block iteratively
    nint i = len(s);
    Word r = default!;
    if (b == 10){
        // hard-coding for 10 here speeds this up by 1.25x (allows for / and % by constants)
        while (len(q) > 0) {
            // extract least significant, base bb "digit"
            (q, r) = q.divW(q, bb);
            for (nint j = 0; j < ndigits && i > 0; j++) {
                i--;
                // avoid % computation since r%10 == r - int(r/10)*10;
                // this appears to be faster for BenchmarkString10000Base10
                // and smaller strings (but a bit slower for larger ones)
                Word t = r / 10;
                s[i] = (byte)((rune)'0' + (byte)(nuint)(r - t * 10));
                r = t;
            }
        }
    } else {
        while (len(q) > 0) {
            // extract least significant, base bb "digit"
            (q, r) = q.divW(q, bb);
            for (nint j = 0; j < ndigits && i > 0; j++) {
                i--;
                s[i] = digits[r % b];
                r /= b;
            }
        }
    }
    // prepend high-order zeros
    while (i > 0) {
        // while need more leading zeros
        i--;
        s[i] = (rune)'0';
    }
}

// Split blocks greater than leafSize Words (or set to 0 to disable recursive conversion)
// Benchmark and configure leafSize using: go test -bench="Leaf"
//
//	8 and 16 effective on 3.0 GHz Xeon "Clovertown" CPU (128 byte cache lines)
//	8 and 16 effective on 2.66 GHz Core 2 Duo "Penryn" CPU
internal static nint leafSize = 8; // number of Word-size binary values treat as a monolithic block

partial struct divisor {
    internal nat bbb; // divisor
    internal nint nbits; // bit length of divisor (discounting leading zeros) ~= log2(bbb)
    internal nint ndigits; // digit length of divisor in terms of output base digits
}


partial struct cacheBase10ᴛ1 /*dyn*/ {
    public partial ref sync_package.Mutex Mutex { get; }
    internal array<divisor> table = new(64); // cached divisors for base 10
}
internal static ж<cacheBase10ᴛ1> ᏑcacheBase10 = new StandardBox<cacheBase10ᴛ1>(new cacheBase10ᴛ1(nil));
internal static ref cacheBase10ᴛ1 cacheBase10 => ref ᏑcacheBase10.Value;

// expWW computes x**y
internal static nat expWW(this nat z, Word x, Word y) {
    return z.expNN(((nat)default!).setWord(x), ((nat)default!).setWord(y), default!, false);
}

// construct table of powers of bb*leafSize to use in subdivisions.
internal static slice<divisor> divisors(nint m, Word b, nint ndigits, Word bb) {
    // only compute table when recursive conversion is enabled and x is large
    if (leafSize == 0 || m <= leafSize) {
        return default!;
    }
    // determine k where (bb**leafSize)**(2**k) >= sqrt(x)
    nint k = 1;
    for (nint words = leafSize; words < (m >> (int)(1)) && k < len(cacheBase10.table); words <<= (int)(1)) {
        k++;
    }
    // reuse and extend existing table of divisors or create new table as appropriate
    slice<divisor> table = default!;               // for b == 10, table overlaps with cacheBase10.table
    if (b == 10){
        ᏑcacheBase10.of(cacheBase10ᴛ1.ᏑMutex).Lock();
        table = cacheBase10.table.slice(0, k); // reuse old table for this conversion
    } else {
        table = new slice<divisor>(k); // create new table for this conversion
    }
    // extend table
    if (table[k - 1].ndigits == 0) {
        // add new entries as needed
        nat larger = default!;
        for (nint i = 0; i < k; i++) {
            if (table[i].ndigits == 0) {
                if (i == 0){
                    table[0].bbb = ((nat)default!).expWW(bb, ((Word)(nuint)leafSize));
                    table[0].ndigits = ndigits * leafSize;
                } else {
                    table[i].bbb = ((nat)default!).sqr(table[i - 1].bbb);
                    table[i].ndigits = 2 * table[i - 1].ndigits;
                }
                // optimization: exploit aggregated extra bits in macro blocks
                larger = ((nat)default!).set(table[i].bbb);
                while (mulAddVWW(larger, larger, b, 0) == 0) {
                    table[i].bbb = table[i].bbb.set(larger);
                    table[i].ndigits++;
                }
                table[i].nbits = table[i].bbb.bitLen();
            }
        }
    }
    if (b == 10) {
        ᏑcacheBase10.of(cacheBase10ᴛ1.ᏑMutex).Unlock();
    }
    return table;
}

} // end big_package
