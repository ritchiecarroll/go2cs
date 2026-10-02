// NilType.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System;

namespace go;

/// <summary>
/// Represents the "nil" type.
/// </summary>
public class NilType : IConvertible
{
    public static NilType Default = null!;

    public override int GetHashCode()
    {
        return 0;
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            NilType          => true,
            ISlice slice     => slice == Default,
            IArray array     => array == Default,
            IChannel channel => channel == Default,
            @string gostr    => gostr == Default,
            string str       => str == Default,
            _                => obj is null
        };
    }

    public override string ToString()
    {
        return "<nil>";
    }

    // IArray to nil comparisons
    public static bool operator ==(IArray? array, NilType _)
    {
        return array is null || array.Length == 0;
    }

    public static bool operator !=(IArray? array, NilType nil)
    {
        return !(array == nil);
    }

    public static bool operator ==(NilType nil, IArray? array)
    {
        return array == nil;
    }

    public static bool operator !=(NilType nil, IArray? array)
    {
        return array != nil;
    }

    // ISlice to nil comparisons — REPRESENTATION nilness: only a genuinely-null reference is nil.
    // A boxed ISlice value is a real slice header and is never nil, even when empty — matching Go
    // (an interface holding an empty or nil slice is itself != nil) and slice<T>'s own == NilType (a
    // CONCRETE slice<T> binds that operator, not this interface one, so this path never observes a
    // representation-nil value). The former `{ Length: 0, Capacity: 0, Source: null }` arm was
    // unreachable dead code: slice<T>.Source (and every wrapper's IArray.Source) materializes a
    // DETACHED copy (ToSpan().ToArray()) and is never null, so the property pattern could not match —
    // the expression already reduced to `slice is null`. (See docs/ConversionStrategies-Reference.md,
    // "Nil-vs-empty slice identity".)
    public static bool operator ==(ISlice? slice, NilType _)
    {
        return slice is null;
    }

    public static bool operator !=(ISlice? slice, NilType nil)
    {
        return !(slice == nil);
    }

    public static bool operator ==(NilType nil, ISlice? slice)
    {
        return slice == nil;
    }

    public static bool operator !=(NilType nil, ISlice? slice)
    {
        return slice != nil;
    }

    // IChannel to nil comparisons — REPRESENTATION nilness: only a genuinely-null reference is nil.
    // The former `{ Length: 0, Capacity: 0 }` pattern predates real unbuffered channels: under the
    // old model a live channel always had Capacity >= 1, so the pattern could only match a boxed
    // ZERO-value channel struct. Now that `make(chan T)` really has cap 0, an EMPTY UNBUFFERED
    // channel would satisfy it and be misclassified as nil. A boxed channel value is never nil in
    // Go anyway (an interface holding even a nil channel is itself != nil), and a CONCRETE
    // channel<T> (or named channel wrapper) binds its own == NilType operator, never this
    // interface one — the same representation-nilness ruling ISlice received above.
    public static bool operator ==(IChannel? channel, NilType _)
    {
        return channel is null;
    }

    public static bool operator !=(IChannel? channel, NilType nil)
    {
        return !(channel == nil);
    }

    public static bool operator ==(NilType nil, IChannel? channel)
    {
        return channel == nil;
    }

    public static bool operator !=(NilType nil, IChannel? channel)
    {
        return channel != nil;
    }

    // string to nil comparisons
    public static bool operator ==(string? obj, NilType _)
    {
        return string.IsNullOrEmpty(obj);
    }

    public static bool operator !=(string? obj, NilType _)
    {
        return !string.IsNullOrEmpty(obj);
    }

    public static bool operator ==(NilType _, string? obj)
    {
        return string.IsNullOrEmpty(obj);
    }

    public static bool operator !=(NilType _, string? obj)
    {
        return !string.IsNullOrEmpty(obj);
    }

    public static implicit operator string(NilType _)
    {
        return "";
        // In Go, string defaults to empty string, not null
    }

    // object to nil comparisons
    public static bool operator ==(object? obj, NilType _)
    {
        return obj is null;
    }

    public static bool operator !=(object? obj, NilType _)
    {
        return obj is not null;
    }

    public static bool operator ==(NilType _, object? obj)
    {
        return obj is null;
    }

    public static bool operator !=(NilType _, object? obj)
    {
        return obj is not null;
    }

    TypeCode IConvertible.GetTypeCode()
    {
        return TypeCode.DBNull;
    }

    bool IConvertible.ToBoolean(IFormatProvider? provider)
    {
        return default;
    }

    byte IConvertible.ToByte(IFormatProvider? provider)
    {
        return default;
    }

    char IConvertible.ToChar(IFormatProvider? provider)
    {
        return default;
    }

    DateTime IConvertible.ToDateTime(IFormatProvider? provider)
    {
        return default;
    }

    decimal IConvertible.ToDecimal(IFormatProvider? provider)
    {
        return default;
    }

    double IConvertible.ToDouble(IFormatProvider? provider)
    {
        return default;
    }

    short IConvertible.ToInt16(IFormatProvider? provider)
    {
        return default;
    }

    int IConvertible.ToInt32(IFormatProvider? provider)
    {
        return default;
    }

    long IConvertible.ToInt64(IFormatProvider? provider)
    {
        return default;
    }

    sbyte IConvertible.ToSByte(IFormatProvider? provider)
    {
        return default;
    }

    float IConvertible.ToSingle(IFormatProvider? provider)
    {
        return default;
    }

    string IConvertible.ToString(IFormatProvider? provider)
    {
        return "";
    }

    object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
    {
        return Default;
    }

    ushort IConvertible.ToUInt16(IFormatProvider? provider)
    {
        return default;
    }

    uint IConvertible.ToUInt32(IFormatProvider? provider)
    {
        return default;
    }

    ulong IConvertible.ToUInt64(IFormatProvider? provider)
    {
        return default;
    }
}
