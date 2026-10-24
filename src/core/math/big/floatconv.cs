// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements string-to-Float conversion functions.
namespace go.math;

using fmt = fmt_package;
using io = io_package;
using strings = strings_package;

partial class big_package {

internal static ж<Float> ᏑfloatZero = new StaticBox<Float>(default(Float));
internal static ref Float floatZero => ref ᏑfloatZero.Value;

// SetString sets z to the value of s and returns z and a boolean indicating
// success. s must be a floating-point number of the same format as accepted
// by [Float.Parse], with base argument 0. The entire string (not just a prefix) must
// be valid for success. If the operation failed, the value of z is undefined
// but the returned value is nil.
public static (ж<Float>, bool) SetString(this ж<Float> Ꮡz, @string s) {
    {
        var (f, _, err) = Ꮡz.Parse(s, 0); if (err == default!) {
            return (f, true);
        }
    }
    return (default!, false);
}

// scan is like Parse but reads the longest possible prefix representing a valid
// floating point number from an io.ByteScanner rather than a string. It serves
// as the implementation of Parse. It does not recognize ±Inf and does not expect
// EOF at the end.
internal static (ж<Float> f, nint b, error err) scan(this ж<Float> Ꮡz, io.ByteScanner r, nint @base) {
    ж<Float> f = default!;
    nint b = default!;
    error err = default!;
    ref var z = ref Ꮡz.DerefOrNull();

    var prec = z.prec;
    if (prec == 0) {
        prec = 64;
    }

    // A reasonable value in case of an error.
    z.form = zero;

    // sign
    (z.neg, err) = scanSign(r);
    if (err != default!) {
        return (f, b, err);
    }

    // mantissa
    nint fcount = default!;  // fractional digit count; valid if <= 0
    (z.mant, b, fcount, err) = z.mant.scan(r, @base, true);
    if (err != default!) {
        return (f, b, err);
    }

    // exponent
    int64 exp = default!;
    nint ebase = default!;
    (exp, ebase, err) = scanExponent(r, true, @base == 0);
    if (err != default!) {
        return (f, b, err);
    }

    // special-case 0
    if (len(z.mant) == 0) {
        z.prec = prec;
        z.acc = Exact;
        z.form = zero;
        f = Ꮡz;
        return (f, b, err);
    }
    // len(z.mant) > 0

    // The mantissa may have a radix point (fcount <= 0) and there
    // may be a nonzero exponent exp. The radix point amounts to a
    // division by b**(-fcount). An exponent means multiplication by
    // ebase**exp. Finally, mantissa normalization (shift left) requires
    // a correcting multiplication by 2**(-shiftcount). Multiplications
    // are commutative, so we can apply them in any order as long as there
    // is no loss of precision. We only have powers of 2 and 10, and
    // we split powers of 10 into the product of the same powers of
    // 2 and 5. This reduces the size of the multiplication factor
    // needed for base-10 exponents.

    // normalize mantissa and determine initial exponent contributions
    var exp2 = (int64)len(z.mant) * (int64)_W - fnorm(z.mant);
    var exp5 = (int64)0;

    // determine binary or decimal exponent contribution of radix point
    if (fcount < 0) {
        // The mantissa has a radix point ddd.dddd; and
        // -fcount is the number of digits to the right
        // of '.'. Adjust relevant exponent accordingly.
        var d = (int64)fcount;
        var exprᴛ1 = b;
        var matchᴛ1 = false;
        if (exprᴛ1 is 10) { matchᴛ1 = true;
            exp5 = d; // 10**e == 5**e * 2**e
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 2) { matchᴛ1 = true;
            exp2 += d;
        }
        else if (exprᴛ1 is 8) { matchᴛ1 = true;
            exp2 += d * 3; // octal digits are 3 bits each
        }
        else if (exprᴛ1 is 16) {
            exp2 += d * 4; // hexadecimal digits are 4 bits each
        }
        else if (!matchᴛ1) { /* default: */
            throw panic("unexpected mantissa base");
        }
        // fcount consumed - not needed anymore
    }

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

    // apply 2**exp2
    if (MinExp <= exp2 && exp2 <= MaxExp){
        z.prec = prec;
        z.form = finite;
        z.exp = (int32)exp2;
        f = Ꮡz;
    } else {
        err = fmt.Errorf("exponent overflow"u8);
        return (f, b, err);
    }

    if (exp5 == 0) {
        // no decimal exponent contribution
        Ꮡz.round(0);
        return (f, b, err);
    }
    // exp5 != 0

    // apply 5**exp5
    var p = @new<Float>().SetPrec(z.Prec() + 64); // use more bits for p -- TODO(gri) what is the right number?
    if (exp5 < 0){
        Ꮡz.Quo(Ꮡz, p.pow5((uint64)(-exp5)));
    } else {
        Ꮡz.Mul(Ꮡz, p.pow5((uint64)exp5));
    }

    return (f, b, err);
}

// These powers of 5 fit into a uint64.
//
//	for p, q := uint64(0), uint64(1); p < q; p, q = q, q*5 {
//		fmt.Println(q)
//	}
[GoLazy] partial class lazyᴛpow5tab { internal static array<uint64> pow5tab = new uint64[]{
    1,
    5,
    25,
    125,
    625,
    3125,
    15625,
    78125,
    390625,
    1953125,
    9765625,
    48828125,
    244140625,
    1220703125,
    6103515625UL,
    30517578125UL,
    152587890625UL,
    762939453125UL,
    3814697265625UL,
    19073486328125UL,
    95367431640625UL,
    476837158203125UL,
    2384185791015625UL,
    11920928955078125UL,
    59604644775390625UL,
    298023223876953125UL,
    1490116119384765625UL,
    7450580596923828125UL
}.array(); }

// pow5 sets z to 5**n and returns z.
// n must not be negative.
internal static ж<Float> pow5(this ж<Float> Ꮡz, uint64 n) {
    ref var z = ref Ꮡz.DerefOrNull();

    const uint64 m = /* uint64(len(pow5tab) - 1) */ 27;
    if (n <= m) {
        return Ꮡz.SetUint64(pow5tab[n]);
    }
    // n > m

    Ꮡz.SetUint64(pow5tab[m]);
    n -= m;

    // use more bits for f than for z
    // TODO(gri) what is the right number?
    var f = @new<Float>().SetPrec(z.Prec() + 64).SetUint64(5);

    while (n > 0) {
        if ((n & 1) != 0) {
            Ꮡz.Mul(Ꮡz, f);
        }
        f.Mul(f, f);
        n >>= (int)(1);
    }

    return Ꮡz;
}

// Parse parses s which must contain a text representation of a floating-
// point number with a mantissa in the given conversion base (the exponent
// is always a decimal number), or a string representing an infinite value.
//
// For base 0, an underscore character “_” may appear between a base
// prefix and an adjacent digit, and between successive digits; such
// underscores do not change the value of the number, or the returned
// digit count. Incorrect placement of underscores is reported as an
// error if there are no other errors. If base != 0, underscores are
// not recognized and thus terminate scanning like any other character
// that is not a valid radix point or digit.
//
// It sets z to the (possibly rounded) value of the corresponding floating-
// point value, and returns z, the actual base b, and an error err, if any.
// The entire string (not just a prefix) must be consumed for success.
// If z's precision is 0, it is changed to 64 before rounding takes effect.
// The number must be of the form:
//
//	number    = [ sign ] ( float | "inf" | "Inf" ) .
//	sign      = "+" | "-" .
//	float     = ( mantissa | prefix pmantissa ) [ exponent ] .
//	prefix    = "0" [ "b" | "B" | "o" | "O" | "x" | "X" ] .
//	mantissa  = digits "." [ digits ] | digits | "." digits .
//	pmantissa = [ "_" ] digits "." [ digits ] | [ "_" ] digits | "." digits .
//	exponent  = ( "e" | "E" | "p" | "P" ) [ sign ] digits .
//	digits    = digit { [ "_" ] digit } .
//	digit     = "0" ... "9" | "a" ... "z" | "A" ... "Z" .
//
// The base argument must be 0, 2, 8, 10, or 16. Providing an invalid base
// argument will lead to a run-time panic.
//
// For base 0, the number prefix determines the actual base: A prefix of
// “0b” or “0B” selects base 2, “0o” or “0O” selects base 8, and
// “0x” or “0X” selects base 16. Otherwise, the actual base is 10 and
// no prefix is accepted. The octal prefix "0" is not supported (a leading
// "0" is simply considered a "0").
//
// A "p" or "P" exponent indicates a base 2 (rather than base 10) exponent;
// for instance, "0x1.fffffffffffffp1023" (using base 0) represents the
// maximum float64 value. For hexadecimal mantissae, the exponent character
// must be one of 'p' or 'P', if present (an "e" or "E" exponent indicator
// cannot be distinguished from a mantissa digit).
//
// The returned *Float f is nil and the value of z is valid but not
// defined if an error is reported.
public static (ж<Float> f, nint b, error err) Parse(this ж<Float> Ꮡz, @string s, nint @base) {
    ж<Float> f = default!;
    nint b = default!;
    error err = default!;

    // scan doesn't handle ±Inf
    if (len(s) == 3 && (s == "Inf"u8 || s == "inf"u8)) {
        f = Ꮡz.SetInf(false);
        return (f, b, err);
    }
    if (len(s) == 4 && (s[0] == (rune)'+' || s[0] == (rune)'-') && (s[1..] == "Inf" || s[1..] == "inf")) {
        f = Ꮡz.SetInf(s[0] == (rune)'-');
        return (f, b, err);
    }

    var r = strings.NewReader(s);
    {
        (f, b, err) = Ꮡz.scan(new strings_ReaderжByteScanner(r), @base); if (err != default!) {
            return (f, b, err);
        }
    }

    // entire string must have been consumed
    {
        var (ch, err2) = r.ReadByte(); if (err2 == default!){
            err = fmt.Errorf("expected end of string, found %q"u8, ch);
        } else 
        if (!AreEqual(err2, io.EOF)) {
            err = err2;
        }
    }

    return (f, b, err);
}

// ParseFloat is like f.Parse(s, base) with f set to the given precision
// and rounding mode.
public static (ж<Float> f, nint b, error err) ParseFloat(@string s, nint @base, nuint prec, RoundingMode mode) {
    return @new<Float>().SetPrec(prec).SetMode(mode).Parse(s, @base);
}

internal static fmt.Scanner _ᴛ2ʗ = new FloatжScanner(((ж<Float>)nil)); // *Float must implement fmt.Scanner

// Scan is a support routine for [fmt.Scanner]; it sets z to the value of
// the scanned number. It accepts formats whose verbs are supported by
// [fmt.Scan] for floating point values, which are:
// 'b' (binary), 'e', 'E', 'f', 'F', 'g' and 'G'.
// Scan doesn't handle ±Inf.
public static error Scan(this ж<Float> Ꮡz, fmt.ScanState s, rune ch) {
    s.SkipSpace();
    var (_, _, err) = Ꮡz.scan(new byteReader(s), 0);
    return err;
}

} // end big_package
