// builtin.TypeParamConversions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace go;

// ---------------------------------------------------------------------------------------------
// TYPE-PARAMETER INTEGER CONVERSIONS — `ConvertToType<T>` / `ConvertToUInt64`, plus the
// `TypeParamCaster<T>` reflection fallback behind them.
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
//   `TypeParamCaster<T>` — genuinely reflective, genuinely slow — sits at the end as the fallback
//   for named wrapper types that carry a numeric field.
//
// SEMANTICS
//   Signed kinds sign-extend and unsigned kinds zero-extend, matching Go integer conversion
//   exactly; every cast is `unchecked`, because Go conversion truncates and never panics.
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
    // sign-extend, unsigned kinds zero-extend - exactly Go integer conversion semantics.
    public static T ConvertToType<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.NonPublicConstructors |
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.NonPublicProperties |
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.NonPublicFields
    )] T>(ulong value) where T : new()
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

        return TypeParamCaster<T>.FromUInt64(value);
    }

    public static T ConvertToType<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.NonPublicConstructors |
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.NonPublicProperties |
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.NonPublicFields
    )] T>(long value) where T : new() => ConvertToType<T>(unchecked((ulong)value));

    public static ulong ConvertToUInt64<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.NonPublicConstructors |
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.NonPublicProperties |
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.NonPublicFields
    )] T>(T value) where T : new()
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

        return TypeParamCaster<T>.ToUInt64(value);
    }

    // Reflection-cached bridge for a numeric wrapper instantiation: reads/writes the wrapper
    // through its Value member and single-argument constructor. A generated [GoType("num:*")]
    // wrapper exposes Value as a PROPERTY; handwritten wrappers (golib uintptr, the
    // managed-referent manual types) expose a FIELD so Interlocked/Volatile seams can take
    // `ref x.Value` - probe both. Static per-T caches keep the reflection cost to first use.
    //
    // THE BINDING FLAGS ARE LOAD-BEARING, and public-only was a latent bug that the W3
    // accessibility arc made reachable. A wrapper for a Go-UNEXPORTED named type is emitted with
    // `internal` members — `internal stringID(uint64 value)` and `internal uint64 Value` — so a
    // public-only probe reads them as ABSENT and the caster throws "no numeric wrapper surface",
    // which names a missing member rather than an invisible one. Measured on
    // NamedNumericOperatorConstraint (`type stringID uint64`; the NAME says string, the TYPE is a
    // named unsigned).
    //
    // The fix is on the PROBE, not on the generated accessibility, and that direction is the whole
    // point: `Value` and the single-argument constructor are golib MARSHALLING surface — Go has no
    // such member, and ConvertToType is reached only from golib's own element conversion (the `copy`
    // path between named types). Promoting them to public to satisfy a probe would widen the C#
    // surface for something Go never asked for, which is the never-more-permissive-than-Go rule
    // pointing the other way. A probe reading members it owns is not a permission question.
    private static class TypeParamCaster<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.NonPublicConstructors |
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.NonPublicProperties |
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.NonPublicFields
    )] T>
    {
        private const BindingFlags ValueSurface = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly PropertyInfo? s_valueProperty = typeof(T).GetProperty("Value", ValueSurface);
        private static readonly FieldInfo? s_valueField = s_valueProperty is null ? typeof(T).GetField("Value", ValueSurface) : null;
        private static readonly Type? s_valueType = s_valueProperty?.PropertyType ?? s_valueField?.FieldType;
        private static readonly ConstructorInfo? s_valueCtor = s_valueType is null
            ? null
            : typeof(T).GetConstructor(ValueSurface, binder: null, [s_valueType], modifiers: null);

        public static T FromUInt64(ulong value)
        {
            if (s_valueType is null || s_valueCtor is null)
                throw new NotSupportedException($"ConvertToType: no numeric wrapper surface on {typeof(T)}");

            // Convert.ChangeType cannot target the native-sized kinds (nint/nuint lack IConvertible)
            object underlying =
                s_valueType == typeof(nint) ? unchecked((nint)value) :
                s_valueType == typeof(nuint) ? unchecked((nuint)value) :
                Convert.ChangeType(unchecked((long)value), s_valueType);

            return (T)s_valueCtor.Invoke([underlying]);
        }

        public static ulong ToUInt64(T value)
        {
            object underlying;

            if (s_valueProperty is not null)
                underlying = s_valueProperty.GetValue(value)!;
            else if (s_valueField is not null)
                underlying = s_valueField.GetValue(value)!;
            else
                throw new NotSupportedException($"ConvertToUInt64: no numeric wrapper surface on {typeof(T)}");

            return underlying switch
            {
                nint ni => unchecked((ulong)ni),
                nuint nu => nu,
                _ => unchecked((ulong)Convert.ToInt64(underlying))
            };
        }
    }

    #endregion
}
