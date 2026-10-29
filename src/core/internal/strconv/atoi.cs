// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class strconv_package {

// lower(c) is a lower-case letter if and only if
// c is either that lower-case letter or the equivalent upper-case letter.
// Instead of writing c == 'x' || c == 'X' one can write lower(c) == 'x'.
// Note that lower of non-letters can produce other non-letters.
internal static byte lower(byte c) {
    return (byte)(c | ((rune)'x' - (rune)'X'));
}

partial struct ΔError /*num:nint*/;

internal static ΔError _ᴛ1ʗ => /* iota */ 0;
public static ΔError ErrRange => 1;
public static ΔError ErrSyntax => 2;
public static ΔError ErrBase => 3;
public static ΔError ErrBitSize => 4;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string valueOutOfRangeˢ = "value out of range"u8;
internal static readonly @string invalidSyntaxˢ = "invalid syntax"u8;
internal static readonly @string invalidBaseˢ = "invalid base"u8;
internal static readonly @string invalidBitSizeˢ = "invalid bit size"u8;
internal static readonly @string unknownErrorˢ = "unknown error"u8;

public static @string Error(this ΔError e) {
    switch (e) {
    case var ᴛ when ᴛ == ErrRange: {
        return valueOutOfRangeˢ;
    }
    case var ᴛ when ᴛ == ErrSyntax: {
        return invalidSyntaxˢ;
    }
    case var ᴛ when ᴛ == ErrBase: {
        return invalidBaseˢ;
    }
    case var ᴛ when ᴛ == ErrBitSize: {
        return invalidBitSizeˢ;
    }}
    return unknownErrorˢ;
}

internal static UntypedInt intSize => /* 32 << (^uint(0) >> 63) */ 64;

// IntSize is the size in bits of an int or uint value.
public static UntypedInt IntSize => /* intSize */ 64;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fnParseUintᶜ = "ParseUint"u8;

// ParseUint is like [ParseInt] but for unsigned numbers.
//
// A sign prefix is not permitted.
public static (uint64, error) ParseUint(@string s, nint @base, nint bitSize) {
    @string fnParseUint = fnParseUintᶜ;

    if (s == ""u8) {
        return (0, ErrSyntax);
    }

    var base0 = @base == 0;

    @string s0 = s;
    switch (ᐧ) {
    case {} when 2 <= @base && @base <= 36: {
        // valid base; nothing to do
        break;
    }
    case {} when @base is 0: {
        // Look for octal, hex prefix.
        @base = 10;
        if (s[0] == '0') {
            switch (ᐧ) {
            case {} when len(s) >= 3 && lower(s[1]) == (byte)'b': {
                @base = 2;
                s = s[2..];
                break;
            }
            case {} when len(s) >= 3 && lower(s[1]) == (byte)'o': {
                @base = 8;
                s = s[2..];
                break;
            }
            case {} when len(s) >= 3 && lower(s[1]) == (byte)'x': {
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
        return (0, ErrBase);
    }}

    if (bitSize == 0) {
        bitSize = IntSize;
    } else if (bitSize < 0 || bitSize > 64) {
        return (0, ErrBitSize);
    }

    // Cutoff is the smallest number such that cutoff*base > maxUint64.
    // Use compile-time constants for common cases.
    uint64 cutoff = default;
    switch (@base) {
    case 10: {
        cutoff = /* maxUint64/10 + 1 */ 1844674407370955162UL;
        break;
    }
    case 16: {
        cutoff = /* maxUint64/16 + 1 */ 1152921504606846976UL;
        break;
    }
    default: {
        cutoff = (uint64)maxUint64 / (uint64)@base + 1;
        break;
    }}

    var maxVal = ((uint64)1).Lsh((nuint)bitSize) - 1;

    var underscores = false;
    uint64 n = default;
    foreach (var (_, c) in slice<byte>(s)) {
        byte d = default;
        switch (ᐧ) {
        case {} when c == (byte)'_' && base0: {
            underscores = true;
            continue;
            break;
        }
        case {} when (byte)'0' <= c && c <= (byte)'9': {
            d = (byte)(c - '0');
            break;
        }
        case {} when (byte)'a' <= lower(c) && lower(c) <= (byte)'z': {
            d = (byte)(lower(c) - 'a' + 10);
            break;
        }
        default: {
            return (0, ErrSyntax);
        }}

        if (d >= (byte)@base) {
            return (0, ErrSyntax);
        }

        if (n >= cutoff) {
            // n*base overflows
            return (maxVal, ErrRange);
        }
        n *= (uint64)@base;

        var n1 = n + (uint64)d;
        if (n1 < n || n1 > maxVal) {
            // n+d overflows
            return (maxVal, ErrRange);
        }
        n = n1;
    }

    if (underscores && !underscoreOK(s0)) {
        return (0, ErrSyntax);
    }

    return (n, default);
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
    error err = default;

    @string fnParseInt = fnParseIntᶜ;

    if (s == ""u8) {
        return (0, ErrSyntax);
    }

    // Pick off leading sign.
    var neg = false;
    switch (s[0]) {
    case (byte)'+': {
        s = s[1..];
        break;
    }
    case (byte)'-': {
        s = s[1..];
        neg = true;
        break;
    }}

    // Convert unsigned and check range.
    uint64 un = default;
    (un, err) = ParseUint(s, @base, bitSize);
    if (err != nil && !AreEqual(err, ErrRange)) {
        return (0, err);
    }

    if (bitSize == 0) {
        bitSize = IntSize;
    }

    var cutoff = (uint64)(((uint64)1).Lsh((nuint)(bitSize - 1)));
    if (!neg && un >= cutoff) {
        return ((int64)(cutoff - 1), ErrRange);
    }
    if (neg && un > cutoff) {
        return (-(int64)cutoff, ErrRange);
    }
    var n = (int64)un;
    if (neg) {
        n = -n;
    }
    return (n, default);
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
        if (s[0] == '-' || s[0] == '+') {
            s = s[1..];
            if (len(s) < 1) {
                return (0, ErrSyntax);
            }
        }

        nint n = 0;
        foreach (var (_, vᴛ1) in slice<byte>(s)) {
            var ch = vᴛ1;

            ch -= (byte)'0';
            if (ch > 9) {
                return (0, ErrSyntax);
            }
            n = n * 10 + (nint)ch;
        }
        if (s0[0] == '-') {
            n = -n;
        }
        return (n, default);
    }

    // Slow path for invalid, big, or underscored integers.
    var (i64, err) = ParseInt(s, 10, 0);
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
    if (len(s) >= 1 && (s[0] == '-' || s[0] == '+')) {
        s = s[1..];
    }

    // Optional base prefix.
    var hex = false;
    if (len(s) >= 2 && s[0] == '0' && (lower(s[1]) == 'b' || lower(s[1]) == 'o' || lower(s[1]) == 'x')) {
        i = 2;
        saw = '0'; // base prefix counts as a digit for "underscore as digit separator"
        hex = lower(s[1]) == 'x';
    }

    // Number proper.
    for (; i < len(s); i++) {
        // Digits are always okay.
        if ('0' <= s[i] && s[i] <= '9' || hex && 'a' <= lower(s[i]) && lower(s[i]) <= 'f') {
            saw = '0';
            continue;
        }
        // Underscore must follow digit.
        if (s[i] == '_') {
            if (saw != '0') {
                return false;
            }
            saw = '_';
            continue;
        }
        // Underscore must also be followed by digit.
        if (saw == '_') {
            return false;
        }
        // Saw non-digit, non-underscore.
        saw = '!';
    }
    return saw != '_';
}

} // end strconv_package
