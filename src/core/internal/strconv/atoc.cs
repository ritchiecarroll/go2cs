// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class strconv_package {

// ParseComplex converts the string s to a complex number
// with the precision specified by bitSize: 64 for complex64, or 128 for complex128.
// When bitSize=64, the result still has type complex128, but it will be
// convertible to complex64 without changing its value.
//
// The number represented by s must be of the form N, Ni, or N±Ni, where N stands
// for a floating-point number as recognized by [ParseFloat], and i is the imaginary
// component. If the second N is unsigned, a + sign is required between the two components
// as indicated by the ±. If the second N is NaN, only a + sign is accepted.
// The form may be parenthesized and cannot contain any spaces.
// The resulting complex number consists of the two components converted by ParseFloat.
//
// The errors that ParseComplex returns have concrete type [*NumError]
// and include err.Num = s.
//
// If s is not syntactically well-formed, ParseComplex returns err.Err = ErrSyntax.
//
// If s is syntactically well-formed but either component is more than 1/2 ULP
// away from the largest floating point number of the given component's size,
// ParseComplex returns err.Err = ErrRange and c = ±Inf for the respective component.
public static (complex128, error) ParseComplex(@string s, nint bitSize) {
    nint size = 64;
    if (bitSize == 64) {
        size = 32; // complex64 uses float32 parts
    }

    // Remove parentheses, if any.
    if (len(s) >= 2 && s[0] == (rune)'(' && s[len(s) - 1] == (rune)')') {
        s = s.slice(1, len(s) - 1);
    }

    error pending = default!;     // pending range error, or nil

    // Read real part (possibly imaginary part if followed by 'i').
    var (re, n, err) = parseFloatPrefix(s, size);
    if (err != default!) {
        if (!AreEqual(err, ErrRange)) {
            return (0D, err);
        }
        pending = err;
    }
    s = s.slice(n);

    // If we have nothing left, we're done.
    if (len(s) == 0) {
        return (complex(re, 0D), pending);
    }

    // Otherwise, look at the next character.
    var exprᴛ1 = s[0];
    var matchᴛ1 = false;
    if (exprᴛ1 is (rune)'+') { matchᴛ1 = true;
        // Consume the '+' to avoid an error if we have "+NaNi", but
        // do this only if we don't have a "++" (don't hide that error).
        if (len(s) > 1 && s[1] != (rune)'+') {
            s = s[1..];
        }
    }
    else if (exprᴛ1 is (rune)'-') { matchᴛ1 = true;
        // ok
    }
    else if (exprᴛ1 is (rune)'i') { matchᴛ1 = true;
        // If 'i' is the last character, we only have an imaginary part.
        if (len(s) == 1) {
            return (complex(0D, re), pending);
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        return (0D, ErrSyntax);
    }

    // Read imaginary part.
    (var im, n, err) = parseFloatPrefix(s, size);
    if (err != default!) {
        if (!AreEqual(err, ErrRange)) {
            return (0D, err);
        }
        pending = err;
    }
    s = s.slice(n);
    if (s != "i"u8) {
        return (0D, ErrSyntax);
    }
    return (complex(re, im), pending);
}

} // end strconv_package
