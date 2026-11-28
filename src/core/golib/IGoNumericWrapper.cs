// IGoNumericWrapper.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go.golib;

/// <summary>
/// The conversions a NAMED Go numeric type takes through a type parameter, which go2cs-gen implements on every
/// <c>[GoType("num:…")]</c> wrapper it generates: <c>T(x)</c> and <c>uint64(v)</c> where <c>T</c> is instantiated with
/// <c>type Celsius float64</c> or <c>type stringID uint64</c>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="builtin.ConvertToType{T}(ulong)"/> and its siblings settle every BCL numeric kind with a <c>typeof(T)</c>
/// ladder the JIT folds per instantiation. A named numeric type is a struct wrapper over one of those kinds, and the
/// wrapper itself knows its kind, so it converts through the same ladder for that kind and wraps or unwraps the result.
/// golib reaches the wrapper through this interface, with no reflection, so a trimmed or Native AOT program keeps the
/// conversion without a <c>DynamicallyAccessedMembers</c> annotation on every converted type parameter that flows into
/// it.
/// </para>
/// <para>
/// Every member takes its operand as an argument and ignores the receiver: golib calls them on one boxed
/// <c>default(TSelf)</c> per type, because a type parameter that is only <c>new()</c>-constrained cannot reach a static
/// member, and so a conversion boxes nothing. Go has no such member, so the wrapper implements the interface explicitly
/// and no Go method name is taken.
/// </para>
/// </remarks>
/// <typeparam name="TSelf">The implementing wrapper.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IGoNumericWrapper<TSelf>
{
    /// <summary>Go's <c>T(x)</c> of an unsigned integer <paramref name="value"/>, as <see cref="builtin.ConvertToType{T}(ulong)"/> converts it.</summary>
    TSelf FromUInt64(ulong value);

    /// <summary>Go's <c>T(x)</c> of a signed integer <paramref name="value"/>, as <see cref="builtin.ConvertToType{T}(long)"/> converts it.</summary>
    TSelf FromInt64(long value);

    /// <summary>Go's <c>T(f)</c> of a float <paramref name="value"/>, as <see cref="builtin.ConvertToType{T}(double)"/> converts it.</summary>
    TSelf FromFloat64(double value);

    /// <summary>Go's <c>uint64(v)</c> of <paramref name="value"/>, as <see cref="builtin.ConvertToUInt64{T}(T)"/> converts it.</summary>
    ulong ToUInt64(TSelf value);

    /// <summary>Go's <c>float64(v)</c> of <paramref name="value"/>, as <see cref="builtin.ConvertToFloat64{T}(T)"/> converts it.</summary>
    double ToFloat64(TSelf value);

    /// <summary>Go's <c>float32(v)</c> of <paramref name="value"/>, as <see cref="builtin.ConvertToFloat32{T}(T)"/> converts it.</summary>
    float ToFloat32(TSelf value);
}
