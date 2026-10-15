// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using strconv = @internal.strconv_package;
using stringslite = @internal.stringslite_package;
using @internal;

partial class strconv_package {

// IntSize is the size in bits of an int or uint value.
public static UntypedInt IntSize => /* strconv.IntSize */ 64;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string parseBoolˢ = "ParseBool"u8;

// ParseBool returns the boolean value represented by the string.
// It accepts 1, t, T, TRUE, true, True, 0, f, F, FALSE, false, False.
// Any other value returns an error.
public static (bool, error) ParseBool(@string str) {
    var (x, err) = strconv.ParseBool(str);
    if (err != default!) {
        return (x, toError(parseBoolˢ, str, 0, 0, err));
    }
    return (x, default!);
}

// FormatBool returns "true" or "false" according to the value of b.
public static @string FormatBool(bool b) {
    return strconv.FormatBool(b);
}

// AppendBool appends "true" or "false", according to the value of b,
// to dst and returns the extended buffer.
public static slice<byte> AppendBool(slice<byte> dst, bool b) {
    return strconv.AppendBool(dst, b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string parseComplexˢ = "ParseComplex"u8;

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
    var (x, err) = strconv.ParseComplex(s, bitSize);
    if (err != default!) {
        return (x, toError(parseComplexˢ, s, 0, bitSize, err));
    }
    return (x, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string parseFloatˢ = "ParseFloat"u8;

// ParseFloat converts the string s to a floating-point number
// with the precision specified by bitSize: 32 for float32, or 64 for float64.
// When bitSize=32, the result still has type float64, but it will be
// convertible to float32 without changing its value.
//
// ParseFloat accepts decimal and hexadecimal floating-point numbers
// as defined by the Go syntax for [floating-point literals].
// If s is well-formed and near a valid floating-point number,
// ParseFloat returns the nearest floating-point number rounded
// using IEEE754 unbiased rounding.
// (Parsing a hexadecimal floating-point value only rounds when
// there are more bits in the hexadecimal representation than
// will fit in the mantissa.)
//
// The errors that ParseFloat returns have concrete type *NumError
// and include err.Num = s.
//
// If s is not syntactically well-formed, ParseFloat returns err.Err = ErrSyntax.
//
// If s is syntactically well-formed but is more than 1/2 ULP
// away from the largest floating point number of the given size,
// ParseFloat returns f = ±Inf, err.Err = ErrRange.
//
// ParseFloat recognizes the string "NaN", and the (possibly signed) strings "Inf" and "Infinity"
// as their respective special floating point values. It ignores case when matching.
//
// [floating-point literals]: https://go.dev/ref/spec#Floating-point_literals
public static (float64, error) ParseFloat(@string s, nint bitSize) {
    var (x, err) = strconv.ParseFloat(s, bitSize);
    if (err != default!) {
        return (x, toError(parseFloatˢ, s, 0, bitSize, err));
    }
    return (x, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string parseUintˢ = "ParseUint"u8;

// ParseUint is like [ParseInt] but for unsigned numbers.
//
// A sign prefix is not permitted.
public static (uint64, error) ParseUint(@string s, nint @base, nint bitSize) {
    var (x, err) = strconv.ParseUint(s, @base, bitSize);
    if (err != default!) {
        return (x, toError(parseUintˢ, s, @base, bitSize, err));
    }
    return (x, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string parseIntˢ = "ParseInt"u8;

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

    (var x, err) = strconv.ParseInt(s, @base, bitSize);
    if (err != default!) {
        return (x, toError(parseIntˢ, s, @base, bitSize, err));
    }
    return (x, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string atoiˢ = "Atoi"u8;

// Atoi is equivalent to ParseInt(s, 10, 0), converted to type int.
public static (nint, error) Atoi(@string s) {
    var (x, err) = strconv.Atoi(s);
    if (err != default!) {
        return (x, toError(atoiˢ, s, 0, 0, err));
    }
    return (x, default!);
}

// FormatComplex converts the complex number c to a string of the
// form (a+bi) where a and b are the real and imaginary parts,
// formatted according to the format fmt and precision prec.
//
// The format fmt and precision prec have the same meaning as in [FormatFloat].
// It rounds the result assuming that the original was obtained from a complex
// value of bitSize bits, which must be 64 for complex64 and 128 for complex128.
public static @string FormatComplex(complex128 c, byte fmt, nint prec, nint bitSize) {
    return strconv.FormatComplex(c, fmt, prec, bitSize);
}

// FormatFloat converts the floating-point number f to a string,
// according to the format fmt and precision prec. It rounds the
// result assuming that the original was obtained from a floating-point
// value of bitSize bits (32 for float32, 64 for float64).
//
// The format fmt is one of
//   - 'b' (-ddddp±ddd, a binary exponent),
//   - 'e' (-d.dddde±dd, a decimal exponent),
//   - 'E' (-d.ddddE±dd, a decimal exponent),
//   - 'f' (-ddd.dddd, no exponent),
//   - 'g' ('e' for large exponents, 'f' otherwise),
//   - 'G' ('E' for large exponents, 'f' otherwise),
//   - 'x' (-0xd.ddddp±ddd, a hexadecimal fraction and binary exponent), or
//   - 'X' (-0Xd.ddddP±ddd, a hexadecimal fraction and binary exponent).
//
// The precision prec controls the number of digits (excluding the exponent)
// printed by the 'e', 'E', 'f', 'g', 'G', 'x', and 'X' formats.
// For 'e', 'E', 'f', 'x', and 'X', it is the number of digits after the decimal point.
// For 'g' and 'G' it is the maximum number of significant digits (trailing
// zeros are removed).
// The special precision -1 uses the smallest number of digits
// necessary such that ParseFloat will return f exactly.
// The exponent is written as a decimal integer;
// for all formats other than 'b', it will be at least two digits.
public static @string FormatFloat(float64 f, byte fmt, nint prec, nint bitSize) {
    return strconv.FormatFloat(f, fmt, prec, bitSize);
}

// AppendFloat appends the string form of the floating-point number f,
// as generated by [FormatFloat], to dst and returns the extended buffer.
public static slice<byte> AppendFloat(slice<byte> dst, float64 f, byte fmt, nint prec, nint bitSize) {
    return strconv.AppendFloat(dst, f, fmt, prec, bitSize);
}

// FormatUint returns the string representation of i in the given base,
// for 2 <= base <= 36. The result uses the lower-case letters 'a' to 'z'
// for digit values >= 10.
public static @string FormatUint(uint64 i, nint @base) {
    return strconv.FormatUint(i, @base);
}

// FormatInt returns the string representation of i in the given base,
// for 2 <= base <= 36. The result uses the lower-case letters 'a' to 'z'
// for digit values >= 10.
public static @string FormatInt(int64 i, nint @base) {
    return strconv.FormatInt(i, @base);
}

// Itoa is equivalent to [FormatInt](int64(i), 10).
public static @string Itoa(nint i) {
    return strconv.Itoa(i);
}

// AppendInt appends the string form of the integer i,
// as generated by [FormatInt], to dst and returns the extended buffer.
public static slice<byte> AppendInt(slice<byte> dst, int64 i, nint @base) {
    return strconv.AppendInt(dst, i, @base);
}

// AppendUint appends the string form of the unsigned integer i,
// as generated by [FormatUint], to dst and returns the extended buffer.
public static slice<byte> AppendUint(slice<byte> dst, uint64 i, nint @base) {
    return strconv.AppendUint(dst, i, @base);
}

// toError converts from internal/strconv.Error to the error guaranteed by this package's APIs.
internal static error toError(@string fn, @string s, nint @base, nint bitSize, error err) {
    var exprᴛ1 = err;
    if (AreEqual(exprᴛ1, strconv.ErrSyntax)) {
        return new NumErrorжerror(syntaxError(fn, s));
    }
    if (AreEqual(exprᴛ1, strconv.ErrRange)) {
        return new NumErrorжerror(rangeError(fn, s));
    }
    if (AreEqual(exprᴛ1, strconv.ErrBase)) {
        return new NumErrorжerror(baseError(fn, s, @base));
    }
    if (AreEqual(exprᴛ1, strconv.ErrBitSize)) {
        return new NumErrorжerror(bitSizeError(fn, s, bitSize));
    }
    return err;
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

} // end strconv_package
