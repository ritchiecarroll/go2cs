// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements rat-to-string conversion functions.
namespace go.math;

using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;

partial class big_package {

internal static bool ratTok(rune ch) {
    return strings.ContainsRune("+-/0123456789.eE"u8, ch);
}

internal static ж<ΔRat> ᏑratZero = new StandardBox<ΔRat>(default(ΔRat));
internal static ref ΔRat ratZero => ref ᏑratZero.Value;

internal static fmt.Scanner _ᴛ6ʗ = new ΔRatжScanner(ᏑratZero); // *Rat must implement fmt.Scanner

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string efgEFGvˢ = "efgEFGv"u8;
internal static readonly @string ratScanInvalidVerbˢ = "Rat.Scan: invalid verb"u8;
internal static readonly @string ratScanInvalidSyntaxˢ = "Rat.Scan: invalid syntax"u8;

// Scan is a support routine for fmt.Scanner. It accepts the formats
// 'e', 'E', 'f', 'F', 'g', 'G', and 'v'. All formats are equivalent.
public static error Scan(this ж<ΔRat> Ꮡz, fmt.ScanState s, rune ch) {
    var (tok, err) = s.Token(true, ratTok);
    if (err != default!) {
        return err;
    }
    if (!strings.ContainsRune(efgEFGvˢ, ch)) {
        return errors.New(ratScanInvalidVerbˢ);
    }
    {
        var (_, ok) = Ꮡz.SetString(((@string)tok)); if (!ok) {
            return errors.New(ratScanInvalidSyntaxˢ);
        }
    }
    return default!;
}

// SetString sets z to the value of s and returns z and a boolean indicating
// success. s can be given as a (possibly signed) fraction "a/b", or as a
// floating-point number optionally followed by an exponent.
// If a fraction is provided, both the dividend and the divisor may be a
// decimal integer or independently use a prefix of “0b”, “0” or “0o”,
// or “0x” (or their upper-case variants) to denote a binary, octal, or
// hexadecimal integer, respectively. The divisor may not be signed.
// If a floating-point number is provided, it may be in decimal form or
// use any of the same prefixes as above but for “0” to denote a non-decimal
// mantissa. A leading “0” is considered a decimal leading 0; it does not
// indicate octal representation in this case.
// An optional base-10 “e” or base-2 “p” (or their upper-case variants)
// exponent may be provided as well, except for hexadecimal floats which
// only accept an (optional) “p” exponent (because an “e” or “E” cannot
// be distinguished from a mantissa digit). If the exponent's absolute value
// is too large, the operation may fail.
// The entire string, not just a prefix, must be valid for success. If the
// operation failed, the value of z is undefined but the returned value is nil.
public static (ж<ΔRat>, bool) SetString(this ж<ΔRat> Ꮡz, @string s) {
    ref var z = ref Ꮡz.DerefOrNull();

    if (len(s) == 0) {
        return (default!, false);
    }
    // len(s) > 0
    // parse fraction a/b, if any
    {
        nint sep = strings.Index(s, "/"u8); if (sep >= 0) {
            {
                var (_, ok) = Ꮡz.of(big_package.ΔRat.Ꮡa).SetString(s.slice(0, sep), 0); if (!ok) {
                    return (default!, false);
                }
            }
            var rΔ1 = strings.NewReader(s.slice(sep + 1));
            error errΔ1 = default!;
            {
                (z.b.abs, _, _, errΔ1) = z.b.abs.scan(new strings_ReaderжByteScanner(rΔ1), 0, false); if (errΔ1 != default!) {
                    return (default!, false);
                }
            }
            // entire string must have been consumed
            {
                (_, errΔ1) = rΔ1.ReadByte(); if (!AreEqual(errΔ1, io.EOF)) {
                    return (default!, false);
                }
            }
            if (len(z.b.abs) == 0) {
                return (default!, false);
            }
            return (Ꮡz.norm(), true);
        }
    }
    // parse floating-point number
    var r = strings.NewReader(s);
    // sign
    var (neg, err) = scanSign(new strings_ReaderжByteScanner(r));
    if (err != default!) {
        return (default!, false);
    }
    // mantissa
    nint @base = default!;
    nint fcount = default!;  // fractional digit count; valid if <= 0
    (z.a.abs, @base, fcount, err) = z.a.abs.scan(new strings_ReaderжByteScanner(r), 0, true);
    if (err != default!) {
        return (default!, false);
    }
    // exponent
    int64 exp = default!;
    nint ebase = default!;
    (exp, ebase, err) = scanExponent(new strings_ReaderжByteScanner(r), true, true);
    if (err != default!) {
        return (default!, false);
    }
    // there should be no unread characters left
    {
        (_, err) = r.ReadByte(); if (!AreEqual(err, io.EOF)) {
            return (default!, false);
        }
    }
    // special-case 0 (see also issue #16176)
    if (len(z.a.abs) == 0) {
        return (Ꮡz.norm(), true);
    }
    // len(z.a.abs) > 0
    // The mantissa may have a radix point (fcount <= 0) and there
    // may be a nonzero exponent exp. The radix point amounts to a
    // division by base**(-fcount), which equals a multiplication by
    // base**fcount. An exponent means multiplication by ebase**exp.
    // Multiplications are commutative, so we can apply them in any
    // order. We only have powers of 2 and 10, and we split powers
    // of 10 into the product of the same powers of 2 and 5. This
    // may reduce the size of shift/multiplication factors or
    // divisors required to create the final fraction, depending
    // on the actual floating-point value.
    // determine binary or decimal exponent contribution of radix point
    int64 exp2 = default!;
    int64 exp5 = default!;
    if (fcount < 0) {
        // The mantissa has a radix point ddd.dddd; and
        // -fcount is the number of digits to the right
        // of '.'. Adjust relevant exponent accordingly.
        var d = (int64)fcount;
        var exprᴛ1 = @base;
        var matchᴛ1 = false;
        if (exprᴛ1 is 10) { matchᴛ1 = true;
            exp5 = d; // 10**e == 5**e * 2**e
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 2) { matchᴛ1 = true;
            exp2 = d;
        }
        else if (exprᴛ1 is 8) { matchᴛ1 = true;
            exp2 = d * 3; // octal digits are 3 bits each
        }
        else if (exprᴛ1 is 16) {
            exp2 = d * 4; // hexadecimal digits are 4 bits each
        }
        else if (!matchᴛ1) { /* default: */
            throw panic("unexpected mantissa base");
        }

    }
    // fcount consumed - not needed anymore
    // take actual exponent into account
    var exprᴛ2 = ebase;
    var matchᴛ2 = false;
    if (exprᴛ2 is 10) { matchᴛ2 = true;
        exp5 += exp; // see fallthrough above
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ2 && exprᴛ2 is 2) {
        exp2 += exp;
    }
    else if (!matchᴛ2) { /* default: */
        throw panic("unexpected exponent base");
    }

    // exp consumed - not needed anymore
    // apply exp5 contributions
    // (start with exp5 so the numbers to multiply are smaller)
    if (exp5 != 0){
        var n = exp5;
        if (n < 0) {
            n = -n;
            if (n < 0) {
                // This can occur if -n overflows. -(-1 << 63) would become
                // -1 << 63, which is still negative.
                return (default!, false);
            }
        }
        if (n > 1000000) {
            return (default!, false); // avoid excessively large exponents
        }
        var pow5 = z.b.abs.expNN(natFive, ((nat)default!).setWord(((Word)(nuint)n)), default!, false); // use underlying array of z.b.abs
        if (exp5 > 0){
            z.a.abs = z.a.abs.mul(z.a.abs, pow5);
            z.b.abs = z.b.abs.setWord(1);
        } else {
            z.b.abs = pow5;
        }
    } else {
        z.b.abs = z.b.abs.setWord(1);
    }
    // apply exp2 contributions
    if (exp2 < -10000000 || exp2 > 10000000) {
        return (default!, false); // avoid excessively large exponents
    }
    if (exp2 > 0){
        z.a.abs = z.a.abs.shl(z.a.abs, (nuint)exp2);
    } else 
    if (exp2 < 0) {
        z.b.abs = z.b.abs.shl(z.b.abs, (nuint)(-exp2));
    }
    z.a.neg = neg && len(z.a.abs) > 0; // 0 has no sign
    return (Ꮡz.norm(), true);
}

// scanExponent scans the longest possible prefix of r representing a base 10
// (“e”, “E”) or a base 2 (“p”, “P”) exponent, if any. It returns the
// exponent, the exponent base (10 or 2), or a read or syntax error, if any.
//
// If sepOk is set, an underscore character “_” may appear between successive
// exponent digits; such underscores do not change the value of the exponent.
// Incorrect placement of underscores is reported as an error if there are no
// other errors. If sepOk is not set, underscores are not recognized and thus
// terminate scanning like any other character that is not a valid digit.
//
//	exponent = ( "e" | "E" | "p" | "P" ) [ sign ] digits .
//	sign     = "+" | "-" .
//	digits   = digit { [ '_' ] digit } .
//	digit    = "0" ... "9" .
//
// A base 2 exponent is only permitted if base2ok is set.
internal static (int64 exp, nint @base, error err) scanExponent(io.ByteScanner r, bool base2ok, bool sepOk) {
    int64 exp = default!;
    nint @base = default!;
    error err = default!;

    // one char look-ahead
    (var ch, err) = r.ReadByte();
    if (err != default!) {
        if (AreEqual(err, io.EOF)) {
            err = default!;
        }
        return (0, 10, err);
    }
    // exponent char
    var exprᴛ1 = ch;
    var matchᴛ1 = false;
    if (exprᴛ1 is (rune)'e' or (rune)'E') { matchᴛ1 = true;
        @base = 10;
    }
    else if (exprᴛ1 is (rune)'p' or (rune)'P') { matchᴛ1 = true;
        do {
            if (base2ok) {
                @base = 2;
                break; // ok
            } // binary exponent not permitted
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        r.UnreadByte(); // ch does not belong to exponent anymore
        return (0, 10, default!);
    }

    // sign
    slice<byte> digits = default!;
    (ch, err) = r.ReadByte();
    if (err == default! && (ch == (rune)'+' || ch == (rune)'-')) {
        if (ch == (rune)'-') {
            digits = append(digits, (byte)((rune)'-'));
        }
        (ch, err) = r.ReadByte();
    }
    // prev encodes the previously seen char: it is one
    // of '_', '0' (a digit), or '.' (anything else). A
    // valid separator '_' may only occur after a digit.
    var prev = (rune)'.';
    var invalSep = false;
    // exponent value
    var hasDigits = false;
    while (err == default!) {
        if ((rune)'0' <= ch && ch <= (rune)'9'){
            digits = append(digits, ch);
            prev = (rune)'0';
            hasDigits = true;
        } else 
        if (ch == (rune)'_' && sepOk){
            if (prev != (rune)'0') {
                invalSep = true;
            }
            prev = (rune)'_';
        } else {
            r.UnreadByte(); // ch does not belong to number anymore
            break;
        }
        (ch, err) = r.ReadByte();
    }
    if (AreEqual(err, io.EOF)) {
        err = default!;
    }
    if (err == default! && !hasDigits) {
        err = errNoDigits;
    }
    if (err == default!) {
        (exp, err) = strconv.ParseInt(((@string)digits), 10, 64);
    }
    // other errors take precedence over invalid separators
    if (err == default! && (invalSep || prev == (rune)'_')) {
        err = errInvalSep;
    }
    return (exp, @base, err);
}

// String returns a string representation of x in the form "a/b" (even if b == 1).
public static @string String(this ж<ΔRat> Ꮡx) {
    return ((@string)Ꮡx.marshal(default!));
}

// marshal implements [Rat.String] returning a slice of bytes.
// It appends the string representation of x in the form "a/b" (even if b == 1) to buf,
// and returns the extended buffer.
internal static slice<byte> marshal(this ж<ΔRat> Ꮡx, slice<byte> buf) {
    ref var x = ref Ꮡx.DerefOrNull();

    buf = Ꮡx.of(big_package.ΔRat.Ꮡa).Append(buf, 10);
    buf = append(buf, (byte)((rune)'/'));
    if (len(x.b.abs) != 0){
        buf = Ꮡx.of(big_package.ΔRat.Ꮡb).Append(buf, 10);
    } else {
        buf = append(buf, (byte)((rune)'1'));
    }
    return buf;
}

// RatString returns a string representation of x in the form "a/b" if b != 1,
// and in the form "a" if b == 1.
public static @string RatString(this ж<ΔRat> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (x.IsInt()) {
        return Ꮡx.of(big_package.ΔRat.Ꮡa).String();
    }
    return Ꮡx.String();
}

// FloatString returns a string representation of x in decimal form with prec
// digits of precision after the radix point. The last digit is rounded to
// nearest, with halves rounded away from zero.
public static @string FloatString(this ж<ΔRat> Ꮡx, nint prec) {
    ref var x = ref Ꮡx.DerefOrNull();

    slice<byte> buf = default!;
    if (x.IsInt()) {
        buf = Ꮡx.of(big_package.ΔRat.Ꮡa).Append(buf, 10);
        if (prec > 0) {
            buf = append(buf, (byte)((rune)'.'));
            for (nint i = prec; i > 0; i--) {
                buf = append(buf, (byte)((rune)'0'));
            }
        }
        return ((@string)buf);
    }
    // x.b.abs != 0
    var (q, r) = ((nat)default!).div(((nat)default!), x.a.abs, x.b.abs);
    var p = natOne;
    if (prec > 0) {
        p = ((nat)default!).expNN(natTen, ((nat)default!).setUint64((uint64)prec), default!, false);
    }
    r = r.mul(r, p);
    (r, var r2) = r.div(((nat)default!), r, x.b.abs);
    // see if we need to round up
    r2 = r2.add(r2, r2);
    if (x.b.abs.cmp(r2) <= 0) {
        r = r.add(r, natOne);
        if (r.cmp(p) >= 0) {
            q = ((nat)default!).add(q, natOne);
            r = ((nat)default!).sub(r, p);
        }
    }
    if (x.a.neg) {
        buf = append(buf, (byte)((rune)'-'));
    }
    buf = appendꓸꓸꓸ(buf, q.utoa(10)); // itoa ignores sign if q == 0
    if (prec > 0) {
        buf = append(buf, (byte)((rune)'.'));
        var rs = r.utoa(10);
        for (nint i = prec - len(rs); i > 0; i--) {
            buf = append(buf, (byte)((rune)'0'));
        }
        buf = appendꓸꓸꓸ(buf, rs);
    }
    return ((@string)buf);
}

// Note: FloatPrec (below) is in this file rather than rat.go because
//       its results are relevant for decimal representation/printing.

// FloatPrec returns the number n of non-repeating digits immediately
// following the decimal point of the decimal representation of x.
// The boolean result indicates whether a decimal representation of x
// with that many fractional digits is exact or rounded.
//
// Examples:
//
//	x      n    exact    decimal representation n fractional digits
//	0      0    true     0
//	1      0    true     1
//	1/2    1    true     0.5
//	1/3    0    false    0       (0.333... rounded)
//	1/4    2    true     0.25
//	1/6    1    false    0.2     (0.166... rounded)
public static (nint n, bool exact) FloatPrec(this ж<ΔRat> Ꮡx) {
    // Determine q and largest p2, p5 such that d = q·2^p2·5^p5.
    // The results n, exact are:
    //
    //     n = max(p2, p5)
    //     exact = q == 1
    //
    // For details see:
    // https://en.wikipedia.org/wiki/Repeating_decimal#Reciprocals_of_integers_not_coprime_to_10
    var d = Ꮡx.Denom().Value.abs; // d >= 1
    // Determine p2 by counting factors of 2.
    // p2 corresponds to the trailing zero bits in d.
    // Do this first to reduce q as much as possible.
    nat q = default!;
    nuint p2 = d.trailingZeroBits();
    q = q.shr(d, p2);
    // Determine p5 by counting factors of 5.
    // Build a table starting with an initial power of 5,
    // and use repeated squaring until the factor doesn't
    // divide q anymore. Then use the table to determine
    // the power of 5 in q.
    const nuint fp = 13; // f == 5^fp
    slice<nat> tab = default!;                // tab[i] == (5^fp)^(2^i) == 5^(fp·2^i)
    var f = new nat(new Word[]{1220703125}.slice()); // == 5^fp (must fit into a uint32 Word)
    nat t = default!;                  // temporaries
    nat r = default!;
    while (ᐧ) {
        {
            (_, r) = t.div(r, q, f); if (len(r) != 0) {
                break; // f doesn't divide q evenly
            }
        }
        tab = append(tab, f);
        f = ((nat)default!).sqr(f); // nat(nil) to ensure a new f for each table entry
    }
    // Factor q using the table entries, if any.
    // We start with the largest factor f = tab[len(tab)-1]
    // that evenly divides q. It does so at most once because
    // otherwise f·f would also divide q. That can't be true
    // because f·f is the next higher table entry, contradicting
    // how f was chosen in the first place.
    // The same reasoning applies to the subsequent factors.
    nuint p5 = default!;
    for (nint i = len(tab) - 1; i >= 0; i--) {
        {
            (t, r) = t.div(r, q, tab[i]); if (len(r) == 0) {
                p5 += fp * (((nuint)1).Lsh((int64)(i))); // tab[i] == 5^(fp·2^i)
                q = q.set(t);
            }
        }
    }
    // If fp != 1, we may still have multiples of 5 left.
    while (ᐧ) {
        {
            (t, r) = t.div(r, q, natFive); if (len(r) != 0) {
                break;
            }
        }
        p5++;
        q = q.set(t);
    }
    return ((nint)max(p2, p5), q.cmp(natOne) == 0);
}

} // end big_package
