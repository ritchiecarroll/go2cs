// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using stringslite = @internal.stringslite_package;
using @internal;

partial class strconv_package {

// lower(c) is a lower-case letter if and only if
// c is either that lower-case letter or the equivalent upper-case letter.
// Instead of writing c == 'x' || c == 'X' one can write lower(c) == 'x'.
// Note that lower of non-letters can produce other non-letters.
internal static byte lower(byte c) {
    return (byte)(c | ((rune)'x' - (rune)'X'));
}

// ErrRange indicates that a value is out of range for the target type.
public static error ErrRange = errors.New("value out of range"u8);

// ErrSyntax indicates that a value does not have the right syntax for the target type.
public static error ErrSyntax = errors.New("invalid syntax"u8);

// A NumError records a failed conversion.
partial struct NumError {
    public @string Func; // the failing function (ParseBool, ParseInt, ParseUint, ParseFloat, ParseComplex)
    public @string Num; // the input
    public error Err;  // the reason the conversion failed (e.g. ErrRange, ErrSyntax, etc.)
}

public static @string Error(this ref NumError e) {
    return "strconv."u8 + e.Func + ": "u8 + "parsing "u8 + Quote(e.Num) + ": "u8 + e.Err.Error();
}

public static error Unwrap(this ref NumError e) {
    return e.Err;
}

// All ParseXXX functions allow the input string to escape to the error value.
// This hurts strconv.ParseXXX(string(b)) calls where b is []byte since
// the conversion from []byte must allocate a string on the heap.
// If we assume errors are infrequent, then we can avoid escaping the input
// back to the output by copying it first. This allows the compiler to call
// strconv.ParseXXX without a heap allocation for most []byte to string
// conversions, since it can now prove that the string cannot escape Parse.
internal static ж<NumError> syntaxError(@string fn, @string str) {
    return Ꮡ(new NumError(fn, stringslite.Clone(str), ErrSyntax));
}

internal static ж<NumError> rangeError(@string fn, @string str) {
    return Ꮡ(new NumError(fn, stringslite.Clone(str), ErrRange));
}

internal static ж<NumError> baseError(@string fn, @string str, nint @base) {
    return Ꮡ(new NumError(fn, stringslite.Clone(str), errors.New("invalid base "u8 + Itoa(@base))));
}

internal static ж<NumError> bitSizeError(@string fn, @string str, nint bitSize) {
    return Ꮡ(new NumError(fn, stringslite.Clone(str), errors.New("invalid bit size "u8 + Itoa(bitSize))));
}

internal static UntypedInt intSize => /* 32 << (^uint(0) >> 63) */ 64;

// IntSize is the size in bits of an int or uint value.
public static UntypedInt IntSize => /* intSize */ 64;

internal static UntypedInt maxUint64 => /* 1<<64 - 1 */ 18446744073709551615;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fnParseUintᶜ = "ParseUint"u8;

// ParseUint is like [ParseInt] but for unsigned numbers.
//
// A sign prefix is not permitted.
public static (uint64, error) ParseUint(@string s, nint @base, nint bitSize) {
    @string fnParseUint = fnParseUintᶜ;
    if (s == ""u8) {
        return (0, new NumErrorжerror(syntaxError(fnParseUint, s)));
    }
    var base0 = @base == 0;
    @string s0 = s;
    switch (ᐧ) {
    case {} when 2 <= @base && @base <= 36: {
        break;
    }
    case {} when @base is 0: {
        @base = 10;
        if (s[0] == (rune)'0') {
            // valid base; nothing to do
            // Look for octal, hex prefix.
            switch (ᐧ) {
            case {} when len(s) >= 3 && lower(s[1]) == (rune)'b': {
                @base = 2;
                s = s[2..];
                break;
            }
            case {} when len(s) >= 3 && lower(s[1]) == (rune)'o': {
                @base = 8;
                s = s[2..];
                break;
            }
            case {} when len(s) >= 3 && lower(s[1]) == (rune)'x': {
                @base = 16;
                s = s[2..];
                break;
            }
            default: {
                @base = 8;
                s = s[1..];
                break;
            }}

        }
        break;
    }
    default: {
        return (0, new NumErrorжerror(baseError(fnParseUint, s0, @base)));
    }}

    if (bitSize == 0){
        bitSize = IntSize;
    } else 
    if (bitSize < 0 || bitSize > 64) {
        return (0, new NumErrorжerror(bitSizeError(fnParseUint, s0, bitSize)));
    }
    // Cutoff is the smallest number such that cutoff*base > maxUint64.
    // Use compile-time constants for common cases.
    uint64 cutoff = default!;
    switch (@base) {
    case 10: {
        cutoff = (uint64)(1844674407370955161L + 1);
        break;
    }
    case 16: {
        cutoff = (uint64)(1152921504606846975L + 1);
        break;
    }
    default: {
        cutoff = (uint64)maxUint64 / (uint64)@base + 1;
        break;
    }}

    var maxVal = ((uint64)1).Lsh((nuint)bitSize) - 1;
    var underscores = false;
    uint64 n = default!;
    foreach (var (_, c) in slice<byte>(s)) {
        byte d = default!;
        switch (ᐧ) {
        case {} when c == (rune)'_' && base0: {
            underscores = true;
            continue;
            break;
        }
        case {} when (rune)'0' <= c && c <= (rune)'9': {
            d = (byte)(c - (rune)'0');
            break;
        }
        case {} when (rune)'a' <= lower(c) && lower(c) <= (rune)'z': {
            d = (byte)(lower(c) - (rune)'a' + 10);
            break;
        }
        default: {
            return (0, new NumErrorжerror(syntaxError(fnParseUint, s0)));
        }}

        if (d >= (byte)@base) {
            return (0, new NumErrorжerror(syntaxError(fnParseUint, s0)));
        }
        if (n >= cutoff) {
            // n*base overflows
            return (maxVal, new NumErrorжerror(rangeError(fnParseUint, s0)));
        }
        n *= (uint64)@base;
        var n1 = n + (uint64)d;
        if (n1 < n || n1 > maxVal) {
            // n+d overflows
            return (maxVal, new NumErrorжerror(rangeError(fnParseUint, s0)));
        }
        n = n1;
    }
    if (underscores && !underscoreOK(s0)) {
        return (0, new NumErrorжerror(syntaxError(fnParseUint, s0)));
    }
    return (n, default!);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fnParseIntᶜ = "ParseInt"u8;

// ParseInt interprets a string s in the given base (0, 2 to 36) and
// bit size (0 to 64) and returns the corresponding value i.
//
// The string may begin with a leading sign: "+" or "-".
//
// If the base argument is 0, the true base is implied by the string's
// prefix following the sign (if present): 2 for "0b", 8 for "0" or "0o",
// 16 for "0x", and 10 otherwise. Also, for argument base 0 only,
// underscore characters are permitted as defined by the Go syntax for
// [integer literals].
//
// The bitSize argument specifies the integer type
// that the result must fit into. Bit sizes 0, 8, 16, 32, and 64
// correspond to int, int8, int16, int32, and int64.
// If bitSize is below 0 or above 64, an error is returned.
//
// The errors that ParseInt returns have concrete type [*NumError]
// and include err.Num = s. If s is empty or contains invalid
// digits, err.Err = [ErrSyntax] and the returned value is 0;
// if the value corresponding to s cannot be represented by a
// signed integer of the given size, err.Err = [ErrRange] and the
// returned value is the maximum magnitude integer of the
// appropriate bitSize and sign.
//
// [integer literals]: https://go.dev/ref/spec#Integer_literals
public static (int64 i, error err) ParseInt(@string s, nint @base, nint bitSize) {
    error err = default!;

    @string fnParseInt = fnParseIntᶜ;
    if (s == ""u8) {
        return (0, new NumErrorжerror(syntaxError(fnParseInt, s)));
    }
    // Pick off leading sign.
    @string s0 = s;
    var neg = false;
    if (s[0] == (rune)'+'){
        s = s[1..];
    } else 
    if (s[0] == (rune)'-') {
        neg = true;
        s = s[1..];
    }
    // Convert unsigned and check range.
    uint64 un = default!;
    (un, err) = ParseUint(s, @base, bitSize);
    if (err != default! && !AreEqual((~err._<ж<NumError>>()).Err, ErrRange)) {
        err._<ж<NumError>>().Value.Func = fnParseInt;
        err._<ж<NumError>>().Value.Num = stringslite.Clone(s0);
        return (0, err);
    }
    if (bitSize == 0) {
        bitSize = IntSize;
    }
    var cutoff = (uint64)(((uint64)1).Lsh((nuint)(bitSize - 1)));
    if (!neg && un >= cutoff) {
        return ((int64)(cutoff - 1), new NumErrorжerror(rangeError(fnParseInt, s0)));
    }
    if (neg && un > cutoff) {
        return (-(int64)cutoff, new NumErrorжerror(rangeError(fnParseInt, s0)));
    }
    var n = (int64)un;
    if (neg) {
        n = -n;
    }
    return (n, default!);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fnAtoiᶜ = "Atoi"u8;

// Atoi is equivalent to ParseInt(s, 10, 0), converted to type int.
public static (nint, error) Atoi(@string s) {
    @string fnAtoi = fnAtoiᶜ;
    nint sLen = len(s);
    if (intSize == 32 && (0 < sLen && sLen < 10) || intSize == 64 && (0 < sLen && sLen < 19)) {
        // Fast path for small integers that fit int type.
        @string s0 = s;
        if (s[0] == (rune)'-' || s[0] == (rune)'+') {
            s = s[1..];
            if (len(s) < 1) {
                return (0, new NumErrorжerror(syntaxError(fnAtoi, s0)));
            }
        }
        nint n = 0;
        foreach (var (_, vᴛ1) in slice<byte>(s)) {
            var ch = vᴛ1;

            ch -= (rune)'0';
            if (ch > 9) {
                return (0, new NumErrorжerror(syntaxError(fnAtoi, s0)));
            }
            n = n * 10 + (nint)ch;
        }
        if (s0[0] == (rune)'-') {
            n = -n;
        }
        return (n, default!);
    }
    // Slow path for invalid, big, or underscored integers.
    var (i64, err) = ParseInt(s, 10, 0);
    {
        var (nerr, ok) = err._<ж<NumError>>(ᐧ); if (ok) {
            nerr.Value.Func = fnAtoi;
        }
    }
    return ((nint)i64, err);
}

// underscoreOK reports whether the underscores in s are allowed.
// Checking them in this one function lets all the parsers skip over them simply.
// Underscore must appear only between digits or between a base prefix and a digit.
internal static bool underscoreOK(@string s) {
    // saw tracks the last character (class) we saw:
    // ^ for beginning of number,
    // 0 for a digit or base prefix,
    // _ for an underscore,
    // ! for none of the above.
    var saw = (rune)'^';
    nint i = 0;
    // Optional sign.
    if (len(s) >= 1 && (s[0] == (rune)'-' || s[0] == (rune)'+')) {
        s = s[1..];
    }
    // Optional base prefix.
    var hex = false;
    if (len(s) >= 2 && s[0] == (rune)'0' && (lower(s[1]) == (rune)'b' || lower(s[1]) == (rune)'o' || lower(s[1]) == (rune)'x')) {
        i = 2;
        saw = (rune)'0'; // base prefix counts as a digit for "underscore as digit separator"
        hex = lower(s[1]) == (rune)'x';
    }
    // Number proper.
    for (; i < len(s); i++) {
        // Digits are always okay.
        if ((rune)'0' <= s[i] && s[i] <= (rune)'9' || hex && (rune)'a' <= lower(s[i]) && lower(s[i]) <= (rune)'f') {
            saw = (rune)'0';
            continue;
        }
        // Underscore must follow digit.
        if (s[i] == (rune)'_') {
            if (saw != (rune)'0') {
                return false;
            }
            saw = (rune)'_';
            continue;
        }
        // Underscore must also be followed by digit.
        if (saw == (rune)'_') {
            return false;
        }
        // Saw non-digit, non-underscore.
        saw = (rune)'!';
    }
    return saw != (rune)'_';
}

} // end strconv_package
