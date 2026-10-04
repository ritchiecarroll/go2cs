// UntypedInt.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;

namespace go;

/// <summary>
/// Represents an untyped integer value.
/// </summary>
public readonly struct UntypedInt : IEquatable<UntypedInt>
{
    // 64-bit value of the struct 'UntypedInt'
    private readonly int64 m_value;

    // Payload bits originated from an unsigned 64-bit constant (e.g., a ulong literal like
    // 18446744073709551615) -- m_value must be reinterpreted as uint64 for value conversions
    private readonly bool m_unsigned;

    /// <summary>
    /// Creates a new <see cref="UntypedInt"/> from <c>int64</c> value.
    /// </summary>
    public UntypedInt(int64 value)
    {
        m_value = value;
    }

    /// <summary>
    /// Creates a new <see cref="UntypedInt"/> from <c>uint64</c> value.
    /// </summary>
    public UntypedInt(uint64 value)
    {
        m_value = CastFrom(value);
        m_unsigned = true;
    }

    // Perform bitwise cast from T to int64
    private static unsafe int64 CastFrom<T>(T source) where T : unmanaged
    {
        Debug.Assert(sizeof(T) <= sizeof(int64), "Type is too large for bitwise conversion to int64");
        void* sourceBits = &source;
        return *(int64*)sourceBits;
    }

    // Perform bitwise cast to T from int64
    private static unsafe T CastTo<T>(int64 source) where T : unmanaged
    {
        Debug.Assert(sizeof(T) <= sizeof(int64), "Type is too large for bitwise conversion from int64");
        void* sourceBits = &source;
        return *(T*)sourceBits;
    }

    // Value (not bitwise) interpretation of the payload for floating-point conversions --
    // a Go untyped int used in a floating-point context converts by value
    private float32 ToFloat32() => m_unsigned ? (float32)CastTo<uint64>(m_value) : (float32)m_value;

    private float64 ToFloat64() => m_unsigned ? (float64)CastTo<uint64>(m_value) : (float64)m_value;

    // The (m_value, m_unsigned) pair denotes a MATHEMATICAL integer: an unsigned-origin payload is
    // the uint64 reinterpretation of the bits (0 … 2^64-1), a signed one is m_value itself. Ordering
    // and equality must be over that VALUE, never over the raw signed bits — a uint64 at or above
    // 2^63 reads as a NEGATIVE int64, so `(uint64)1<<63 < 100` answered TRUE. That is how strconv's
    // `if fastSmalls && i < nSmalls` fast path fired for FormatUint(9223372036854775808): small()
    // then truncated the negative index and returned "0" (TestUitoa), or threw on the slice bound
    // (TestFormatUintVarlen). Only a payload at/above 2^63 changes answer; every signed-only or
    // in-int64-range comparison is bit-for-bit what it was.
    //
    // ⚠ THE ATTRIBUTE IS LOAD-BEARING AND MEASURED, not precautionary (the math/bits word-size
    // arc, 2026-09-02): this body is IL 141 — over the JIT's default inlining budget — so
    // `DOTNET_JitDisasmSummary` showed it compiled STANDALONE and every wrapper comparison in the
    // corpus paid a call per evaluation, measured at 2.72x on `UintSize == 32` in addMulVVW's
    // loop. The converter emits every package-level untyped integer constant as a property of
    // this struct (6,667 declarations in 193 packages; 1,986 comparison sites — census in
    // docs/phase4/DESIGN-untyped-const-emission.md), so the comparison path is corpus-wide.
    // Inlined, the JIT constant-folds through readonly-struct ops and a foldable comparison
    // (`UintSize == 32`) dies at JIT time — Go's own compile-time folding, one stage later.
    // UntypedFloat/UntypedComplex deliberately take NO attribute: read, not assumed — neither
    // has this private Compare, and their operators are one-line m_value compares already
    // within budget. If a future runtime inlines IL-141 bodies unaided, the attribute becomes
    // redundant rather than wrong (the bits_impl.cs precedent).
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static int Compare(UntypedInt left, UntypedInt right)
    {
        if (left.m_unsigned == right.m_unsigned)
        {
            return left.m_unsigned
                ? CastTo<uint64>(left.m_value).CompareTo(CastTo<uint64>(right.m_value))
                : left.m_value.CompareTo(right.m_value);
        }

        // Mixed representations: a NEGATIVE signed payload is below every unsigned payload.
        // Otherwise both denote a non-negative value and the unsigned reading of each is exact.
        int64 signedValue = left.m_unsigned ? right.m_value : left.m_value;

        if (signedValue < 0)
            return left.m_unsigned ? 1 : -1;

        return CastTo<uint64>(left.m_value).CompareTo(CastTo<uint64>(right.m_value));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool Equals(UntypedInt other) => Compare(this, other) == 0;

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            UntypedInt other => Equals(other),
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
            float32 value => ToFloat64() == value,
            float64 value => ToFloat64() == value,
            complex64 value => value == (complex64)this,
            complex128 value => value == (complex128)this,
            _ => false
        };
    }

    public override int GetHashCode() => m_value.GetHashCode();

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator <(UntypedInt left, UntypedInt right) => Compare(left, right) < 0;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(UntypedInt left, UntypedInt right) => Compare(left, right) <= 0;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator >(UntypedInt left, UntypedInt right) => Compare(left, right) > 0;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(UntypedInt left, UntypedInt right) => Compare(left, right) >= 0;

    public static UntypedInt operator +(UntypedInt left, UntypedInt right) => left.m_value + right.m_value;

    public static UntypedInt operator -(UntypedInt left, UntypedInt right) => left.m_value - right.m_value;

    public static UntypedInt operator -(UntypedInt value) => -(int64)value.m_value;

    public static UntypedInt operator *(UntypedInt left, UntypedInt right) => left.m_value * right.m_value;

    public static UntypedInt operator /(UntypedInt left, UntypedInt right) => left.m_value / right.m_value;

    public static UntypedInt operator %(UntypedInt left, UntypedInt right) => left.m_value % right.m_value;

    public static UntypedInt operator <<(UntypedInt left, int right) => left.m_value << right;

    public static UntypedInt operator >>(UntypedInt left, int right) => left.m_value >> right;

    public override string ToString() => m_unsigned ? CastTo<uint64>(m_value).ToString() : m_value.ToString();

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(UntypedInt left, UntypedInt right) => left.Equals(right);

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(UntypedInt left, UntypedInt right) => !(left == right);

    // Handle implicit conversions between 'nint' and struct 'UntypedInt'
    public static implicit operator UntypedInt(nint value) => new(value);

    public static implicit operator nint(UntypedInt value) => (nint)value.m_value;

    // Handle implicit conversions between 'int8' and struct 'UntypedInt'
    public static implicit operator UntypedInt(int8 value) => new(value);

    public static implicit operator int8(UntypedInt value) => (int8)value.m_value;

    // Handle implicit conversions between 'int16' and struct 'UntypedInt'
    public static implicit operator UntypedInt(int16 value) => new(value);

    public static implicit operator int16(UntypedInt value) => (int16)value.m_value;

    // Handle implicit conversions between 'int32' and struct 'UntypedInt'
    public static implicit operator UntypedInt(int32 value) => new(value);

    public static implicit operator int32(UntypedInt value) => (int32)value.m_value;

    // Handle implicit conversions between 'int64' and struct 'UntypedInt'
    public static implicit operator UntypedInt(int64 value) => new(value);

    public static implicit operator int64(UntypedInt value) => value.m_value;

    // Handle implicit conversions between 'nuint' and struct 'UntypedInt'
    public static implicit operator UntypedInt(nuint value) => new((uint64)value);

    public static implicit operator nuint(UntypedInt value) => CastTo<nuint>(value.m_value);

    // Handle implicit conversions between 'uint8' and struct 'UntypedInt'
    public static implicit operator UntypedInt(uint8 value) => new(value);

    public static implicit operator uint8(UntypedInt value) => (uint8)value.m_value;

    // Handle implicit conversions between 'uint16' and struct 'UntypedInt'
    public static implicit operator UntypedInt(uint16 value) => new(value);

    public static implicit operator uint16(UntypedInt value) => (uint16)value.m_value;

    // Handle implicit conversions between 'uint32' and struct 'UntypedInt'
    public static implicit operator UntypedInt(uint32 value) => new(value);

    public static implicit operator uint32(UntypedInt value) => (uint32)value.m_value;

    // Handle implicit conversions between 'uint64' and struct 'UntypedInt'
    public static implicit operator UntypedInt(uint64 value) => new(value);

    public static implicit operator uint64(UntypedInt value) => CastTo<uint64>(value.m_value);

    // FROM a float or complex the conversion is EXPLICIT; TO one it stays implicit. A Go untyped integer
    // constant beside a float operand converts to the FLOAT type (`f >= math.MaxUint64` compares two
    // float64s). C# picks a user-defined operator whenever an implicit conversion reaches its operand
    // type, and only then considers the built-in ones, so an implicit float -> UntypedInt made
    // `f >= c` and `f * c` run UntypedInt's operator on `(int64)f`: saturated at 2^63, a fraction
    // truncated (go-humanize's ParseBytes overflow guard never fired; 0.5 * c read 0). With no implicit
    // conversion from a float, no UntypedInt operator applies, and C# uses the built-in float operator
    // through the implicit UntypedInt -> float below, which rounds the constant as Go does.
    public static explicit operator UntypedInt(float32 value) => new((int64)value);

    public static implicit operator float32(UntypedInt value) => value.ToFloat32();

    // Handle implicit conversions between 'float64' and struct 'UntypedInt'
    public static explicit operator UntypedInt(float64 value) => new((int64)value);

    public static implicit operator float64(UntypedInt value) => value.ToFloat64();

    // Handle implicit conversions between 'complex64' and struct 'UntypedInt'
    public static explicit operator UntypedInt(complex64 value) => new((int64)value.Real);

    public static implicit operator complex64(UntypedInt value) => value.ToFloat32();

    // Handle implicit conversions between 'complex128' and struct 'UntypedInt'
    public static explicit operator UntypedInt(complex128 value) => new((int64)value.Real);

    public static implicit operator complex128(UntypedInt value) => value.ToFloat64();

    // Handle comparisons between 'nil' and struct 'UntypedInt'
    public static bool operator ==(UntypedInt value, NilType nil) => value.Equals(default);

    public static bool operator !=(UntypedInt value, NilType nil) => !(value == nil);

    public static bool operator ==(NilType nil, UntypedInt value) => value == nil;

    public static bool operator !=(NilType nil, UntypedInt value) => value != nil;

    public static implicit operator UntypedInt(NilType nil) => default!;
}
