// complex64.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Globalization;
using System.Numerics;

namespace go;

/// <summary>
/// Represents a numeric type for the set of all complex numbers with float32 real and imaginary parts.
/// </summary>
public readonly struct complex64
    : IConvertible, 
      IEquatable<complex64>, 
      IEqualityOperators<complex64, complex64, bool>,
      IFormattable,
      IGoEquatable
// TODO: Implement new numeric interfaces / validate against System.Numerics.Complex:
/*, 
INumberBase<complex64>,
ISignedNumber<complex64>,
IUtf8SpanFormattable*/
{
    // complex64 implementation derived from .NET Complex source:
    //      https://github.com/Microsoft/referencesource/blob/master/System.Numerics/System/Numerics/Complex.cs
    //      Copyright (c) Microsoft Corporation.  All rights reserved.

    private readonly float m_real;
    private readonly float m_imaginary;

    private const float LOG_10_INV = 0.43429448190325F;

    public float Real
    {
        get
        {
            return m_real;
        }
    }

    public float Imaginary
    {
        get
        {
            return m_imaginary;
        }
    }

    public float Magnitude
    {
        get
        {
            return Abs(this);
        }
    }

    public float Phase
    {
        get
        {
            return (float)Math.Atan2(m_imaginary, m_real);
        }
    }

    public static readonly complex64 Zero = new(0.0F, 0.0F);
    public static readonly complex64 One = new(1.0F, 0.0F);
    public static readonly complex64 ImaginaryOne = new(0.0F, 1.0F);

    public complex64(float real, float imaginary) /* Constructor to create a complex number with rectangular co-ordinates  */
    {
        m_real = real;
        m_imaginary = imaginary;
    }

    public static complex64 FromPolarCoordinates(float magnitude, float phase)
    {
        return new complex64(magnitude * (float)Math.Cos(phase), magnitude * (float)Math.Sin(phase));
    }

    public static complex64 Negate(complex64 value)
    {
        return -value;
    }

    public static complex64 Add(complex64 left, complex64 right)
    {
        return left + right;
    }

    public static complex64 Subtract(complex64 left, complex64 right)
    {
        return left - right;
    }

    public static complex64 Multiply(complex64 left, complex64 right)
    {
        return left * right;
    }

    public static complex64 Divide(complex64 dividend, complex64 divisor)
    {
        return dividend / divisor;
    }

    public static complex64 operator -(complex64 value)
    {
        return new complex64(-value.m_real, -value.m_imaginary);
    }

    public static complex64 operator +(complex64 left, complex64 right)
    {
        return new complex64(left.m_real + right.m_real, left.m_imaginary + right.m_imaginary);
    }

    public static complex64 operator -(complex64 left, complex64 right)
    {
        return new complex64(left.m_real - right.m_real, left.m_imaginary - right.m_imaginary);
    }

    public static complex64 operator *(complex64 left, complex64 right)
    {
        // Multiplication:  (a + bi)(c + di) = (ac -bd) + (bc + ad)i
        float result_Realpart = left.m_real * right.m_real - left.m_imaginary * right.m_imaginary;
        float result_Imaginarypart = left.m_imaginary * right.m_real + left.m_real * right.m_imaginary;
        return new complex64(result_Realpart, result_Imaginarypart);
    }

    public static complex64 operator /(complex64 left, complex64 right)
    {
        // Division : Smith's formula.
        float a = left.m_real;
        float b = left.m_imaginary;
        float c = right.m_real;
        float d = right.m_imaginary;

        if (Math.Abs(d) < Math.Abs(c))
        {
            float doc = d / c;
            return new complex64((a + b * doc) / (c + d * doc), (b - a * doc) / (c + d * doc));
        }

        float cod = c / d;
        return new complex64((b + a * cod) / (d + c * cod), (-a + b * cod) / (d + c * cod));
    }

    public static float Abs(complex64 value)
    {
        if (float.IsInfinity(value.m_real) || float.IsInfinity(value.m_imaginary))
            return float.PositiveInfinity;

        // |value| == sqrt(a^2 + b^2)
        // sqrt(a^2 + b^2) == a/a * sqrt(a^2 + b^2) = a * sqrt(a^2/a^2 + b^2/a^2)
        // Using the above we can factor out the square of the larger component to dodge overflow.

        float c = Math.Abs(value.m_real);
        float d = Math.Abs(value.m_imaginary);
        float r;

        if (c > d)
        {
            r = d / c;
            return c * (float)Math.Sqrt(1.0 + r * r);
        }

        if (d == 0.0F)
            return c; // c is either 0.0 or NaN

        r = c / d;
        return d * (float)Math.Sqrt(1.0 + r * r);
    }

    public static complex64 Conjugate(complex64 value)
    {
        return new complex64(value.m_real, -value.m_imaginary);
    }

    // Reciprocal of a Complex number : the reciprocal of x+i*y is 1/(x+i*y)
    public static complex64 Reciprocal(complex64 value)
    {
        return value.m_real == 0 && value.m_imaginary == 0 ? Zero : One / value;
    }

    // Go's == through an interface (builtin.AreEqual): this type's own operator, explicitly, so no member is added.
    bool IGoEquatable.GoEquals(object? other) => other is complex64 value && this == value;

    public static bool operator ==(complex64 left, complex64 right)
    {
        return left.m_real == right.m_real && left.m_imaginary == right.m_imaginary;
    }

    public static bool operator !=(complex64 left, complex64 right)
    {
        return left.m_real != right.m_real || left.m_imaginary != right.m_imaginary;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not complex64 complex)
            return false;

        return this == complex;
    }

    public bool Equals(complex64 value)
    {
        return m_real.Equals(value.m_real) && m_imaginary.Equals(value.m_imaginary);
    }

    public static implicit operator complex64(short value)
    {
        return new complex64(value, 0.0F);
    }

    public static implicit operator complex64(int value)
    {
        return new complex64(value, 0.0F);
    }

    public static implicit operator complex64(long value)
    {
        return new complex64(value, 0.0F);
    }

    public static implicit operator complex64(ushort value)
    {
        return new complex64(value, 0.0F);
    }

    public static implicit operator complex64(uint value)
    {
        return new complex64(value, 0.0F);
    }

    public static implicit operator complex64(ulong value)
    {
        return new complex64(value, 0.0F);
    }

    public static implicit operator complex64(sbyte value)
    {
        return new complex64(value, 0.0F);
    }

    public static implicit operator complex64(byte value)
    {
        return new complex64(value, 0.0F);
    }

    public static implicit operator complex64(float value)
    {
        return new complex64(value, 0.0F);
    }

    public static explicit operator complex64(double value)
    {
        return new complex64((float)value, 0.0F);
    }

    public static explicit operator complex64(BigInteger value)
    {
        return new complex64((float)value, 0.0F);
    }

    public static explicit operator complex64(decimal value)
    {
        return new complex64((float)value, 0.0F);
    }

    // Enable conversions between Complex and complex64 struct
    public static explicit operator complex64(complex128 value)
    {
        return new complex64((float)value.Real, (float)value.Imaginary);
    }

    public static implicit operator complex128(complex64 value)
    {
        return new complex128(value.m_real, value.m_imaginary);
    }

    // Enable comparisons between nil and complex64 struct
    public static bool operator ==(complex64 value, NilType _)
    {
        return value.Equals(default);
    }

    public static bool operator !=(complex64 value, NilType nil)
    {
        return !(value == nil);
    }

    public static bool operator ==(NilType nil, complex64 value)
    {
        return value == nil;
    }

    public static bool operator !=(NilType nil, complex64 value)
    {
        return value != nil;
    }

    public static implicit operator complex64(NilType _)
    {
        return default;
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.CurrentCulture, "({0}, {1})", m_real, m_imaginary);
    }

    public string ToString(string format)
    {
        return string.Format(CultureInfo.CurrentCulture, "({0}, {1})",
            m_real.ToString(format, CultureInfo.CurrentCulture),
            m_imaginary.ToString(format, CultureInfo.CurrentCulture));
    }

    public string ToString(string? format, IFormatProvider? provider)
    {
        return string.Format(provider, "({0}, {1})", m_real.ToString(format, provider),
            m_imaginary.ToString(format, provider));
    }

    public string ToString(IFormatProvider? provider)
    {
        return string.Format(provider, "({0}, {1})", m_real, m_imaginary);
    }

    public override int GetHashCode()
    {
        int n1 = 99999997;
        int hash_real = m_real.GetHashCode() % n1;
        int hash_imaginary = m_imaginary.GetHashCode();
        int final_hashcode = hash_real ^ hash_imaginary;

        return final_hashcode;
    }

    public static complex64 Sin(complex64 value)
    {
        float a = value.m_real;
        float b = value.m_imaginary;
        return new complex64((float)Math.Sin(a) * (float)Math.Cosh(b), (float)Math.Cos(a) * (float)Math.Sinh(b));
    }

    public static complex64 Sinh(complex64 value)
    {
        float a = value.m_real;
        float b = value.m_imaginary;
        return new complex64((float)Math.Sinh(a) * (float)Math.Cos(b), (float)Math.Cosh(a) * (float)Math.Sin(b));
    }

    public static complex64 Asin(complex64 value)
    {
        return -ImaginaryOne * Log(ImaginaryOne * value + Sqrt(One - value * value));
    }

    public static complex64 Cos(complex64 value)
    {
        float a = value.m_real;
        float b = value.m_imaginary;
        return new complex64((float)Math.Cos(a) * (float)Math.Cosh(b), -((float)Math.Sin(a) * (float)Math.Sinh(b)));
    }

    public static complex64 Cosh(complex64 value)
    {
        float a = value.m_real;
        float b = value.m_imaginary;
        return new complex64((float)Math.Cosh(a) * (float)Math.Cos(b), (float)Math.Sinh(a) * (float)Math.Sin(b));
    }

    public static complex64 Acos(complex64 value)
    {
        return -ImaginaryOne * Log(value + ImaginaryOne * Sqrt(One - value * value));
    }

    public static complex64 Tan(complex64 value)
    {
        return Sin(value) / Cos(value);
    }

    public static complex64 Tanh(complex64 value)
    {
        return Sinh(value) / Cosh(value);
    }

    public static complex64 Atan(complex64 value)
    {
        complex64 Two = new(2.0F, 0.0F);
        return ImaginaryOne / Two * (Log(One - ImaginaryOne * value) - Log(One + ImaginaryOne * value));
    }

    public static complex64 Log(complex64 value)
    {
        return new complex64((float)Math.Log(Abs(value)), (float)Math.Atan2(value.m_imaginary, value.m_real));
    }

    public static complex64 Log(complex64 value, float baseValue)
    {
        return Log(value) / Log(baseValue);
    }

    public static complex64 Log10(complex64 value)
    {
        return Scale(Log(value), LOG_10_INV);
    }

    public static complex64 Exp(complex64 value)
    {
        float temp_factor = (float)Math.Exp(value.m_real);
        float result_re = temp_factor * (float)Math.Cos(value.m_imaginary);
        float result_im = temp_factor * (float)Math.Sin(value.m_imaginary);
        return new complex64(result_re, result_im);
    }

    public static complex64 Sqrt(complex64 value)
    {
        return FromPolarCoordinates((float)Math.Sqrt(value.Magnitude), value.Phase / 2.0F);
    }

    public static complex64 Pow(complex64 value, complex64 power)
    {
        if (power == Zero)
            return One;

        if (value == Zero)
            return Zero;

        float a = value.m_real;
        float b = value.m_imaginary;
        float c = power.m_real;
        float d = power.m_imaginary;

        float rho = Abs(value);
        float theta = (float)Math.Atan2(b, a);
        float newRho = c * theta + d * (float)Math.Log(rho);

        float t = (float)Math.Pow(rho, c) * (float)Math.Pow(Math.E, -d * theta);

        return new complex64(t * (float)Math.Cos(newRho), t * (float)Math.Sin(newRho));
    }

    public static complex64 Pow(complex64 value, float power)
    {
        return Pow(value, new complex64(power, 0));
    }

    private static complex64 Scale(complex64 value, float factor)
    {
        float result_re = factor * value.m_real;
        float result_im = factor * value.m_imaginary;
        return new complex64(result_re, result_im);
    }

    TypeCode IConvertible.GetTypeCode()
    {
        return m_real.GetTypeCode();
    }

    bool IConvertible.ToBoolean(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToBoolean(provider);
    }

    char IConvertible.ToChar(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToChar(provider);
    }

    sbyte IConvertible.ToSByte(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToSByte(provider);
    }

    byte IConvertible.ToByte(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToByte(provider);
    }

    short IConvertible.ToInt16(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToInt16(provider);
    }

    ushort IConvertible.ToUInt16(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToUInt16(provider);
    }

    int IConvertible.ToInt32(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToInt32(provider);
    }

    uint IConvertible.ToUInt32(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToUInt32(provider);
    }

    long IConvertible.ToInt64(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToInt64(provider);
    }

    ulong IConvertible.ToUInt64(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToUInt64(provider);
    }

    float IConvertible.ToSingle(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToSingle(provider);
    }

    double IConvertible.ToDouble(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToDouble(provider);
    }

    decimal IConvertible.ToDecimal(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToDecimal(provider);
    }

    DateTime IConvertible.ToDateTime(IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToDateTime(provider);
    }

    object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
    {
        return ((IConvertible)m_real).ToType(conversionType, provider);
    }
}
