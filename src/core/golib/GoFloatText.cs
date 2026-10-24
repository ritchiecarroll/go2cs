// GoFloatText.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Globalization;
using System.Text;

namespace go;

/// <summary>
/// Go's runtime text for a float or complex value: <c>strconv.AppendFloat(v, 'g', -1, bitSize)</c> and
/// <c>strconv.AppendComplex(c, 'g', -1, bitSize)</c>, which runtime/print.go's printfloat and printcomplex write
/// (and so printpanicval for a panic value).
/// </summary>
/// <remarks>
/// golib sits below the converted <c>strconv</c>, so the 'g' decision is ported here (internal/strconv's
/// formatDigits, fmtE and fmtF). The DIGITS are .NET's round-trip text, which since .NET Core 3.0 is the shortest
/// that parses back at the value's own width: the digits Go's roundShortest picks.
/// </remarks>
internal static class GoFloatText
{
    /// <summary>Formats <paramref name="value"/> as Go's <c>FormatFloat(value, 'g', -1, bitSize)</c>.</summary>
    internal static string Format(double value, int bitSize)
    {
        if (double.IsNaN(value))
            return "NaN";

        if (double.IsInfinity(value))
            return value > 0 ? "+Inf" : "-Inf";

        string text = bitSize == 32 ?
            ((float)value).ToString("R", CultureInfo.InvariantCulture) :
            value.ToString("R", CultureInfo.InvariantCulture);

        // The decimal digits without leading or trailing zeros, and dp, the decimal point's position after
        // digit dp - 1: Go's decimalSlice.
        bool negative = double.IsNegative(value);
        int start = text[0] == '-' ? 1 : 0;
        int exponentAt = text.IndexOf('E');
        int exponent = exponentAt < 0 ? 0 : int.Parse(text.AsSpan(exponentAt + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        string mantissa = text[start..(exponentAt < 0 ? text.Length : exponentAt)];
        int point = mantissa.IndexOf('.');
        string digits = point < 0 ? mantissa : mantissa.Remove(point, 1);
        int dp = (point < 0 ? mantissa.Length : point) + exponent;
        int leading = 0;

        while (leading < digits.Length && digits[leading] == '0')
            leading++;

        digits = digits[leading..].TrimEnd('0');
        dp -= leading;

        if (digits.Length == 0)
            dp = 0;

        StringBuilder result = new();

        if (negative)
            result.Append('-');

        // formatDigits' 'g' arm at the shortest precision: %e when the exponent is below -4 or at least 6.
        int exp = dp - 1;

        if (exp < -4 || exp >= 6)
        {
            result.Append(digits.Length == 0 ? '0' : digits[0]);

            if (digits.Length > 1)
                result.Append('.').Append(digits, 1, digits.Length - 1);

            if (digits.Length == 0)
                exp = 0;

            result.Append('e').Append(exp < 0 ? '-' : '+');
            exp = Math.Abs(exp);

            if (exp < 10)
                result.Append('0');

            return result.Append(exp.ToString(CultureInfo.InvariantCulture)).ToString();
        }

        // fmtF with max(nd - dp, 0) fraction digits.
        if (dp > 0)
        {
            int integer = Math.Min(digits.Length, dp);
            result.Append(digits, 0, integer).Append('0', dp - integer);
        }
        else
        {
            result.Append('0');
        }

        int fraction = Math.Max(digits.Length - dp, 0);

        if (fraction > 0)
        {
            result.Append('.');

            for (int i = 0; i < fraction; i++)
            {
                int j = dp + i;
                result.Append(j >= 0 && j < digits.Length ? digits[j] : '0');
            }
        }

        return result.ToString();
    }

    /// <summary>Formats a complex value as Go's <c>FormatComplex(c, 'g', -1, bitSize)</c>: <c>(re±imi)</c>.</summary>
    internal static string Complex(double real, double imaginary, int bitSize)
    {
        // A complex64's parts are float32s, a complex128's float64s.
        int width = bitSize == 64 ? 32 : 64;
        string im = Format(imaginary, width);

        if (im[0] != '+' && im[0] != '-')
            im = "+" + im;

        return $"({Format(real, width)}{im}i)";
    }
}
