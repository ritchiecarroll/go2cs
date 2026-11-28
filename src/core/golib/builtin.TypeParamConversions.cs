// builtin.TypeParamConversions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go.golib;

namespace go;

// ---------------------------------------------------------------------------------------------
// TYPE-PARAMETER INTEGER CONVERSIONS — `ConvertToType<T>` / `ConvertToUInt64`, plus the
// named-numeric fallback behind them (`IGoNumericWrapper<T>`, implemented by go2cs-gen).
//
// WHY THIS EXISTS
//   Go lets a generic function convert through a constrained type parameter: `T(x)` where
//   `T ~int | ~uint64 | …`. C# cannot express that — a cast to or from an unconstrained type
//   parameter is CS0030 — so the converter emits a call here instead, and the conversion is
//   decided at run time from `typeof(T)`.
//
// WHY IT IS NOT SLOW (do not "simplify" this into a switch on a Type variable)
//   The `typeof(T) == typeof(int)` chain looks like a linear scan but is not: for a value-type T
//   the JIT specializes the method per instantiation, each comparison folds to a compile-time
//   constant, and everything but the one matching branch is dropped. What is left is the single
//   `unchecked` cast the Go source asked for. Rewriting the chain as a runtime `Type` switch or a
//   dictionary lookup would defeat that folding and turn a free conversion into a hash probe on a
//   path the numeric corpus hits constantly.
//
//   The `(T)(object)` round trip does box on paths the JIT cannot fold (a reference-typed or
//   unconstrained T), which is why the ladder is ordered with the common integer kinds first and
//   the named-numeric fallback sits at the end.
//
// NAMED NUMERIC TYPES (`type Celsius float64`, `type stringID uint64`)
//   A named numeric type is a generated struct wrapper over one BCL kind, so no rung of the ladder
//   matches it. The wrapper knows its own kind, and go2cs-gen implements IGoNumericWrapper<T> on it:
//   each member converts through this same ladder for that kind and wraps or unwraps the result
//   (go2cs-gen's NumericTypeTemplate). Reaching it through an interface is what keeps these
//   conversions free of reflection. They were reflective once, through Value and a one-argument
//   constructor, and that made every type parameter flowing in here carry a DynamicallyAccessedMembers
//   annotation the converter never writes: 37 of the 42 IL2091 warnings across 10 behavioral projects
//   at 4a56f17a3a, one per converted generic call site, and a trimmed program could lose the members
//   the reflection read.
//
// SEMANTICS
//   Signed kinds sign-extend and unsigned kinds zero-extend, matching Go integer conversion
//   exactly; every cast is `unchecked`, because Go conversion truncates and never panics. A FLOAT
//   source truncates toward zero, and out of range takes gc's answer for this process's
//   architecture: amd64's "integer indefinite", or arm64's saturation (the `double` overload says how).
//
// WHY IT LIVES IN ITS OWN FILE
//   It sat inside the `func<TRef…>` region in builtin.cs purely by accident of where it was
//   added — it has nothing to do with execution contexts.
// ---------------------------------------------------------------------------------------------
public static partial class builtin
{
    #region Type parameter integer conversions

    // Go type-parameter conversions: `T(x)` / `uint64(n)` where a constrained type parameter is
    // involved cannot be a C# cast (CS0030 - no conversion to/from a type parameter). Runtime-typed
    // dispatch; the typeof checks JIT-fold to a single branch per instantiation. Signed kinds
    // sign-extend, unsigned kinds zero-extend - exactly Go integer conversion semantics. A float kind
    // converts the integer as Go's float64(x) does, so the SOURCE's signedness decides it: the two
    // public overloads share one ladder and say which they were given.
    public static T ConvertToType<T>(ulong value) where T : new() => IntegerToType<T>(value, signed: false);

    public static T ConvertToType<T>(long value) where T : new() => IntegerToType<T>(unchecked((ulong)value), signed: true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T IntegerToType<T>(ulong value, bool signed) where T : new()
    {
        if (typeof(T) == typeof(nint)) return (T)(object)unchecked((nint)value);
        if (typeof(T) == typeof(long)) return (T)(object)unchecked((long)value);
        if (typeof(T) == typeof(int)) return (T)(object)unchecked((int)value);
        if (typeof(T) == typeof(short)) return (T)(object)unchecked((short)value);
        if (typeof(T) == typeof(sbyte)) return (T)(object)unchecked((sbyte)value);
        if (typeof(T) == typeof(nuint)) return (T)(object)unchecked((nuint)value);
        if (typeof(T) == typeof(ulong)) return (T)(object)value;
        if (typeof(T) == typeof(uint)) return (T)(object)unchecked((uint)value);
        if (typeof(T) == typeof(ushort)) return (T)(object)unchecked((ushort)value);
        if (typeof(T) == typeof(byte)) return (T)(object)unchecked((byte)value);
        if (typeof(T) == typeof(uintptr)) return (T)(object)new uintptr(unchecked((nuint)value));
        if (typeof(T) == typeof(double)) return (T)(object)(signed ? (double)unchecked((long)value) : (double)value);
        if (typeof(T) == typeof(float)) return (T)(object)(signed ? (float)unchecked((long)value) : (float)value);

        IGoNumericWrapper<T> wrapper = NumericWrapper<T>("ConvertToType");
        return signed ? wrapper.FromInt64(unchecked((long)value)) : wrapper.FromUInt64(value);
    }

    /// <summary>
    /// Go's <c>T(f)</c> of a float into an integer-constrained type parameter, <c>T(value.(float64))</c> with
    /// <c>T ~int32</c>. An in-range value truncates toward zero, as Go's float-to-integer conversion does. A value out
    /// of range (or NaN) is implementation-defined in Go, and gc's answer differs by GOARCH, so the answer here is the
    /// one gc gives on the architecture this process runs on (both measured with go1.26.10, through a type parameter,
    /// over every integer kind):
    /// <list type="bullet">
    /// <item><b>amd64</b> (and any other architecture): the 64-bit kinds take CVTTSD2SQ, whose NaN or out-of-range
    /// result is the "integer indefinite" value, <c>long.MinValue</c>; int32 takes CVTTSD2SL, likewise
    /// <c>int.MinValue</c>; and the unsigned 64-bit kinds take gc's split at 2^63.</item>
    /// <item><b>arm64</b>: FCVTZS and FCVTZU saturate, and NaN reads 0. int64 and int32 clamp to their range, and the
    /// unsigned 64-bit kinds clamp to [0, MaxUint64].</item>
    /// </list>
    /// On both, the 8- and 16-bit kinds keep the low bits of the int32 result and uint32 the low bits of the int64 one,
    /// because gc converts them through those widths. A C# cast cannot stand in for the amd64 answers, and the arm64 ones
    /// are written out rather than left to the runtime's cast. A float32 argument widens to double exactly, so it
    /// converts the same.
    /// </summary>
    public static T ConvertToType<T>(double value) where T : new()
    {
        if (typeof(T) == typeof(nint)) return (T)(object)(nint)FloatToInt64(value, s_floatConversionSaturates);
        if (typeof(T) == typeof(long)) return (T)(object)FloatToInt64(value, s_floatConversionSaturates);
        if (typeof(T) == typeof(int)) return (T)(object)FloatToInt32(value, s_floatConversionSaturates);
        if (typeof(T) == typeof(short)) return (T)(object)unchecked((short)FloatToInt32(value, s_floatConversionSaturates));
        if (typeof(T) == typeof(sbyte)) return (T)(object)unchecked((sbyte)FloatToInt32(value, s_floatConversionSaturates));
        if (typeof(T) == typeof(nuint)) return (T)(object)(nuint)FloatToUInt64(value, s_floatConversionSaturates);
        if (typeof(T) == typeof(ulong)) return (T)(object)FloatToUInt64(value, s_floatConversionSaturates);
        if (typeof(T) == typeof(uint)) return (T)(object)unchecked((uint)FloatToInt64(value, s_floatConversionSaturates));
        if (typeof(T) == typeof(ushort)) return (T)(object)unchecked((ushort)FloatToInt32(value, s_floatConversionSaturates));
        if (typeof(T) == typeof(byte)) return (T)(object)unchecked((byte)FloatToInt32(value, s_floatConversionSaturates));
        if (typeof(T) == typeof(uintptr)) return (T)(object)new uintptr((nuint)FloatToUInt64(value, s_floatConversionSaturates));
        if (typeof(T) == typeof(double)) return (T)(object)value;
        if (typeof(T) == typeof(float)) return (T)(object)(float)value;

        return NumericWrapper<T>("ConvertToType").FromFloat64(value);
    }

    // Whether this process takes gc's arm64 answers. A static readonly flag folds to a constant in optimized code.
    private static readonly bool s_floatConversionSaturates = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

    // The three conversions every kind is built from. Each range test is written so that NaN fails it.
    //   amd64, CVTTSD2SQ: NaN and every value outside [-2^63, 2^63) read long.MinValue.
    //   arm64, FCVTZS: clamp to [long.MinValue, long.MaxValue], NaN 0.
    internal static long FloatToInt64(double value, bool arm64)
    {
        if (value >= -9223372036854775808.0 && value < 9223372036854775808.0)
            return (long)value;

        if (!arm64)
            return long.MinValue;

        return double.IsNaN(value) ? 0 : value > 0 ? long.MaxValue : long.MinValue;
    }

    //   amd64, CVTTSD2SL: the int32 twin. Anything above -2^31 - 1 truncates to at least int.MinValue, so the lower bound
    //   is open. arm64, FCVTZS to a W register: clamp, NaN 0.
    internal static int FloatToInt32(double value, bool arm64)
    {
        if (value > -2147483649.0 && value < 2147483648.0)
            return (int)value;

        if (!arm64)
            return int.MinValue;

        return double.IsNaN(value) ? 0 : value > 0 ? int.MaxValue : int.MinValue;
    }

    //   amd64: below 2^63, the signed conversion reinterpreted; otherwise the signed conversion of value - 2^63 with the
    //   top bit set. NaN takes the second arm and reads 2^63, as gc's does.
    //   arm64, FCVTZU: clamp to [0, MaxUint64], NaN 0. Anything above -1 truncates to at least 0.
    internal static ulong FloatToUInt64(double value, bool arm64)
    {
        if (!arm64)
        {
            return value < 9223372036854775808.0
                ? unchecked((ulong)FloatToInt64(value, arm64: false))
                : unchecked((ulong)FloatToInt64(value - 9223372036854775808.0, arm64: false)) | 0x8000000000000000UL;
        }

        if (value > -1.0 && value < 18446744073709551616.0)
            return (ulong)value;

        return value > 0 ? ulong.MaxValue : 0;
    }

    // The same conversions keyed by a runtime type, for tests of either architecture's table on any host. Each arm boxes
    // its own kind: without the (object) casts the chain's natural type is double, and every arm would box a double.
    internal static object FloatToKind(double value, Type kind, bool arm64) =>
        kind == typeof(nint) ? (object)(nint)FloatToInt64(value, arm64) :
        kind == typeof(long) ? (object)FloatToInt64(value, arm64) :
        kind == typeof(int) ? (object)FloatToInt32(value, arm64) :
        kind == typeof(short) ? (object)unchecked((short)FloatToInt32(value, arm64)) :
        kind == typeof(sbyte) ? (object)unchecked((sbyte)FloatToInt32(value, arm64)) :
        kind == typeof(nuint) ? (object)(nuint)FloatToUInt64(value, arm64) :
        kind == typeof(ulong) ? (object)FloatToUInt64(value, arm64) :
        kind == typeof(uint) ? (object)unchecked((uint)FloatToInt64(value, arm64)) :
        kind == typeof(ushort) ? (object)unchecked((ushort)FloatToInt32(value, arm64)) :
        kind == typeof(byte) ? (object)unchecked((byte)FloatToInt32(value, arm64)) :
        kind == typeof(uintptr) ? (object)new uintptr((nuint)FloatToUInt64(value, arm64)) :
        kind == typeof(double) ? (object)value :
        kind == typeof(float) ? (object)(float)value :
        throw new NotSupportedException($"ConvertToType: no float conversion to a wrapper over {kind}");

    public static ulong ConvertToUInt64<T>(T value) where T : new()
    {
        if (typeof(T) == typeof(nint)) return unchecked((ulong)(nint)(object)value!);
        if (typeof(T) == typeof(long)) return unchecked((ulong)(long)(object)value!);
        if (typeof(T) == typeof(int)) return unchecked((ulong)(int)(object)value!);
        if (typeof(T) == typeof(short)) return unchecked((ulong)(short)(object)value!);
        if (typeof(T) == typeof(sbyte)) return unchecked((ulong)(sbyte)(object)value!);
        if (typeof(T) == typeof(nuint)) return (nuint)(object)value!;
        if (typeof(T) == typeof(ulong)) return (ulong)(object)value!;
        if (typeof(T) == typeof(uint)) return (uint)(object)value!;
        if (typeof(T) == typeof(ushort)) return (ushort)(object)value!;
        if (typeof(T) == typeof(byte)) return (byte)(object)value!;
        if (typeof(T) == typeof(uintptr)) return ((uintptr)(object)value!).Value;
        if (typeof(T) == typeof(double)) return FloatToUInt64((double)(object)value!, s_floatConversionSaturates);
        if (typeof(T) == typeof(float)) return FloatToUInt64((float)(object)value!, s_floatConversionSaturates);

        return NumericWrapper<T>("ConvertToUInt64").ToUInt64(value);
    }

    /// <summary>
    /// Go's <c>float64(v)</c> of a type-parameter value whose type set holds a FLOAT member, such as math/big's
    /// <c>func csv[T int | float64](…)</c>. A float is the identity or a widening, and an integer is C#'s numeric
    /// conversion, which rounds to nearest as Go's <c>float64(int)</c> does. The ladder folds per instantiation,
    /// as <see cref="ConvertToUInt64{T}"/>'s does.
    /// </summary>
    public static double ConvertToFloat64<T>(T value) where T : new()
    {
        if (typeof(T) == typeof(double)) return (double)(object)value!;
        if (typeof(T) == typeof(float)) return (float)(object)value!;
        if (typeof(T) == typeof(nint)) return (nint)(object)value!;
        if (typeof(T) == typeof(long)) return (long)(object)value!;
        if (typeof(T) == typeof(int)) return (int)(object)value!;
        if (typeof(T) == typeof(short)) return (short)(object)value!;
        if (typeof(T) == typeof(sbyte)) return (sbyte)(object)value!;
        if (typeof(T) == typeof(nuint)) return (nuint)(object)value!;
        if (typeof(T) == typeof(ulong)) return (ulong)(object)value!;
        if (typeof(T) == typeof(uint)) return (uint)(object)value!;
        if (typeof(T) == typeof(ushort)) return (ushort)(object)value!;
        if (typeof(T) == typeof(byte)) return (byte)(object)value!;
        if (typeof(T) == typeof(uintptr)) return ((uintptr)(object)value!).Value;

        return NumericWrapper<T>("ConvertToFloat64").ToFloat64(value);
    }

    /// <summary>
    /// Go's <c>float32(v)</c> of a type-parameter value whose type set holds a FLOAT member. An integer converts to
    /// float32 DIRECTLY, never through float64, so it rounds once as Go's does; a float64 rounds once to float32.
    /// </summary>
    public static float ConvertToFloat32<T>(T value) where T : new()
    {
        if (typeof(T) == typeof(float)) return (float)(object)value!;
        if (typeof(T) == typeof(double)) return (float)(double)(object)value!;
        if (typeof(T) == typeof(nint)) return (nint)(object)value!;
        if (typeof(T) == typeof(long)) return (long)(object)value!;
        if (typeof(T) == typeof(int)) return (int)(object)value!;
        if (typeof(T) == typeof(short)) return (short)(object)value!;
        if (typeof(T) == typeof(sbyte)) return (sbyte)(object)value!;
        if (typeof(T) == typeof(nuint)) return (nuint)(object)value!;
        if (typeof(T) == typeof(ulong)) return (ulong)(object)value!;
        if (typeof(T) == typeof(uint)) return (uint)(object)value!;
        if (typeof(T) == typeof(ushort)) return (ushort)(object)value!;
        if (typeof(T) == typeof(byte)) return (byte)(object)value!;
        if (typeof(T) == typeof(uintptr)) return ((uintptr)(object)value!).Value;

        return NumericWrapper<T>("ConvertToFloat32").ToFloat32(value);
    }

    // The named-numeric fallback: a go2cs-gen wrapper implements IGoNumericWrapper<T> (go2cs-gen's NumericTypeTemplate).
    // Every member takes its operand as an argument and ignores its receiver, so one boxed default(T) per type serves
    // every call, and a conversion allocates nothing. A type that is not such a wrapper (and a reference type, whose
    // default is null) is refused by name, as the reflective probe refused it before.
    private static IGoNumericWrapper<T> NumericWrapper<T>(string caller) =>
        NumericWrapperOf<T>.Instance ?? throw new NotSupportedException($"{caller}: no numeric wrapper surface on {typeof(T)}");

    private static class NumericWrapperOf<T>
    {
        internal static readonly IGoNumericWrapper<T>? Instance = default(T) as IGoNumericWrapper<T>;
    }

    #endregion
}
