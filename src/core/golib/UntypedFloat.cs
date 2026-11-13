// UntypedFloat.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Globalization;

namespace go;

/// <summary>
/// Represents an untyped floating-point value.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public readonly struct UntypedFloat(float64 value) : IEquatable<UntypedFloat>
{
    // Value of the struct 'UntypedFloat'
    private readonly float64 m_value = value;

    // ReSharper disable once CompareOfFloatsByEqualityOperator
    public bool Equals(UntypedFloat other) => m_value == other.m_value;

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            UntypedFloat other => Equals(other),
            nint value => Equals(value),
            int8 value => Equals(value),
            int16 value => Equals(value),
            int32 value => Equals(value),
            int64 value => Equals(value),
            nuint value => Equals(value),
            uint8 value => Equals(value),
            uint16 value => Equals(value),
            uint32 value => Equals(value),
            uint64 value => Equals(value),
            float32 value => Equals(value),
            float64 value => Equals(value),
            complex64 value => Equals(value),
            complex128 value => Equals(value),
            _ => false
        };
    }

    public override int GetHashCode() => m_value.GetHashCode();

    public static bool operator <(UntypedFloat left, UntypedFloat right) => left.m_value < right.m_value;

    public static bool operator <=(UntypedFloat left, UntypedFloat right) => left.m_value <= right.m_value;

    public static bool operator >(UntypedFloat left, UntypedFloat right) => left.m_value > right.m_value;

    public static bool operator >=(UntypedFloat left, UntypedFloat right) => left.m_value >= right.m_value;

    public static UntypedFloat operator +(UntypedFloat left, UntypedFloat right) => left.m_value + right.m_value;

    public static UntypedFloat operator -(UntypedFloat left, UntypedFloat right) => left.m_value - right.m_value;

    public static UntypedFloat operator -(UntypedFloat value) => -value.m_value;

    public static UntypedFloat operator *(UntypedFloat left, UntypedFloat right) => left.m_value * right.m_value;

    public static UntypedFloat operator /(UntypedFloat left, UntypedFloat right) => left.m_value / right.m_value;

    public static UntypedFloat operator %(UntypedFloat left, UntypedFloat right) => left.m_value % right.m_value;

    public override string ToString() => m_value.ToString(CultureInfo.InvariantCulture);

    public static bool operator ==(UntypedFloat left, UntypedFloat right) => left.Equals(right);

    public static bool operator !=(UntypedFloat left, UntypedFloat right) => !(left == right);

    // Handle implicit conversions between 'nint' and struct 'UntypedFloat'
    public static implicit operator UntypedFloat(nint value) => new(value);

    public static implicit operator nint(UntypedFloat value) => (nint)value.m_value;

    // Handle implicit conversions between 'int8' and struct 'UntypedFloat'
    public static implicit operator UntypedFloat(int8 value) => new(value);

    public static implicit operator int8(UntypedFloat value) => (int8)value.m_value;

    // Handle implicit conversions between 'int16' and struct 'UntypedFloat'
    public static implicit operator UntypedFloat(int16 value) => new(value);

    public static implicit operator int16(UntypedFloat value) => (int16)value.m_value;

    // Handle implicit conversions between 'int32' and struct 'UntypedFloat'
    public static implicit operator UntypedFloat(int32 value) => new(value);

    public static implicit operator int32(UntypedFloat value) => (int32)value.m_value;

    // Handle implicit conversions between 'int64' and struct 'UntypedFloat'
    public static implicit operator UntypedFloat(int64 value) => new(value);

    public static implicit operator int64(UntypedFloat value) => (int64)value.m_value;

    // Handle implicit conversions between 'nuint' and struct 'UntypedFloat'
    public static implicit operator UntypedFloat(nuint value) => new(value);

    public static implicit operator nuint(UntypedFloat value) => (nuint)value.m_value;

    // Handle implicit conversions between 'uint8' and struct 'UntypedFloat'
    public static implicit operator UntypedFloat(uint8 value) => new(value);

    public static implicit operator uint8(UntypedFloat value) => (uint8)value.m_value;

    // Handle implicit conversions between 'uint16' and struct 'UntypedFloat'
    public static implicit operator UntypedFloat(uint16 value) => new(value);

    public static implicit operator uint16(UntypedFloat value) => (uint16)value.m_value;

    // Handle implicit conversions between 'uint32' and struct 'UntypedFloat'
    public static implicit operator UntypedFloat(uint32 value) => new(value);

    public static implicit operator uint32(UntypedFloat value) => (uint32)value.m_value;

    // Handle implicit conversions between 'uint64' and struct 'UntypedFloat'
    public static implicit operator UntypedFloat(uint64 value) => new(value);

    public static implicit operator uint64(UntypedFloat value) => (uint64)value.m_value;

    // Handle implicit conversions between 'float32' and struct 'UntypedFloat'
    public static implicit operator UntypedFloat(float32 value) => new(value);

    public static implicit operator float32(UntypedFloat value) => (float32)value.m_value;

    // Handle implicit conversions between 'float64' and struct 'UntypedFloat'
    public static implicit operator UntypedFloat(float64 value) => new(value);

    public static implicit operator float64(UntypedFloat value) => value.m_value;

    // An untyped int WIDENS to an untyped float: Go evaluates `procs * capacityPerProc` (untyped int
    // times untyped float) as an untyped FLOAT. Without this, no user-defined operator applies to the
    // mixed pair, and C# falls back to the predefined `int * int` through both wrappers' implicit
    // numeric conversions: the product wraps at 2^31 and a fractional operand truncates. With it, the
    // mixed pair binds `UntypedFloat op UntypedFloat`. This is one conversion, not mixed operators: an
    // `UntypedFloat op UntypedInt` operator would make every `untypedFloat op typedNumber` ambiguous
    // (the typed operand converts implicitly to both wrappers, CS9342). Only this direction is implicit;
    // `UntypedInt op UntypedInt` never considers it, so integer arithmetic and division stay integer.
    public static implicit operator UntypedFloat(UntypedInt value) => new((float64)value);

    // EXPLICIT (not implicit) in BOTH directions: an untyped float already converts implicitly to
    // float64, so letting it ALSO relate implicitly to a complex type makes every
    // `complexExpr op untypedFloat` (Go's `1i * math.Pi`) ambiguous — the operands can bind either
    // as `Complex op double` (untypedFloat → double) OR as `UntypedFloat op UntypedFloat` (the
    // complex → UntypedFloat conversion), and neither is preferred (CS0034). Keeping the
    // float↔complex relationship explicit in both directions resolves such arithmetic to
    // `Complex op double`; the converter emits an explicit cast where Go assigns an untyped float
    // constant directly to a complex (the rare direction), and complex → UntypedFloat is never a
    // real conversion the converter needs (untyped complex constants are UntypedComplex).
    public static explicit operator UntypedFloat(complex64 value) => new(value.Real);

    public static explicit operator complex64(UntypedFloat value) => (complex64)value.m_value;

    public static explicit operator UntypedFloat(complex128 value) => new(value.Real);

    public static explicit operator complex128(UntypedFloat value) => value.m_value;

    // Handle comparisons between 'nil' and struct 'UntypedFloat'
    public static bool operator ==(UntypedFloat value, NilType nil) => value.Equals(default);

    public static bool operator !=(UntypedFloat value, NilType nil) => !(value == nil);

    public static bool operator ==(NilType nil, UntypedFloat value) => value == nil;

    public static bool operator !=(NilType nil, UntypedFloat value) => value != nil;

    public static implicit operator UntypedFloat(NilType nil) => default!;
}
