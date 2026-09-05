//******************************************************************************************************
//  GoReflect.PointerConversions.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/05/2026 - Increment E3 root 5 (reflect.Value.Convert's pointer family)
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Reflection;
using go.golib;

namespace go;

// The pointer family of reflect.Value.Convert -- `(*[N]T)(s)` and `(*B)(p)` -- built on the SAME
// aliasing machinery the converter's own emissions use, so a Value handed out here obeys the one
// rule the family has: the result NAMES THE SOURCE'S STORAGE, never a copy of it. A copy would pass
// reflect's convertTests, which compare values, while silently dropping every write a caller makes
// through the converted pointer (image/png's TestWriteRGBA is the corpus witness the slice arm's
// original author recorded) -- so where the managed model has no aliasing representation the
// conversion is REFUSED and the caller panics with Go's "cannot be converted" text rather than
// answering wrong.
public static partial class GoReflect
{
    private static readonly ConcurrentDictionary<Type, Func<object, nint, object>> s_sliceArrayAliasers = new();
    private static readonly ConcurrentDictionary<(Type, Type), Func<object, object>?> s_pointerReinterpreters = new();

    /// <summary>
    /// Go's <c>(*[N]T)(s)</c>: a pointer to an array of length <paramref name="length"/> that ALIASES
    /// the slice's backing store (<see cref="array{T}.Alias"/> -- the same window the language
    /// conversion takes). A defined array pointee wraps the aliased header (its elements are shared
    /// through the backing), and a defined POINTER type is the generated class wrapping the box.
    /// The caller has already applied Go's length rule; a nil slice never reaches here (its
    /// conversion is the destination's nil).
    /// </summary>
    public static object AliasSliceAsArrayPointer(object slice, Type pointerType, nint length)
    {
        Type boxType = underlyingPointerType(pointerType);
        Type pointee = boxPointeeType(boxType) ?? throw new InvalidOperationException($"AliasSliceAsArrayPointer: {pointerType} is not a pointer type");
        Type arrayType = wrapperUnderlyingType(pointee) ?? pointee;
        Type elem = ElementType(arrayType) ?? throw new InvalidOperationException($"AliasSliceAsArrayPointer: {pointee} is not an array pointee");

        // A defined slice source (`type MyBytes []byte`) is its wrapper; the alias windows the
        // underlying slice<E> it wraps, so the wrapper's backing is exactly what the array shares.
        object source = TryUnwrapWrapperValue(slice, out object? unwrapped) ? unwrapped : slice;

        object aliased = s_sliceArrayAliasers.GetOrAdd(elem, static et =>
            typeof(GoReflect).GetMethod(nameof(aliasSliceAsArray), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(et).CreateDelegate<Func<object, nint, object>>())(source, length);

        // A defined array pointee (`type MyBytesArray0 [0]byte`) takes the aliased HEADER through its
        // generated single-argument constructor: the header's backing reference is the alias.
        object pointeeValue = aliased;

        if (arrayType != pointee && !TryConvertTo(aliased, pointee, out pointeeValue!))
            throw new InvalidOperationException($"AliasSliceAsArrayPointer: cannot wrap {arrayType} as {pointee}");

        return wrapPointerBox(NewPointerBox(pointee, pointeeValue), pointerType);
    }

    private static object aliasSliceAsArray<E>(object source, nint length)
    {
        return source switch
        {
            slice<E> s => array<E>.Alias(s, length),
            ISlice<E> view => array<E>.Alias(new slice<E>(view), length),
            _ => throw new InvalidOperationException($"AliasSliceAsArrayPointer: unsupported slice {source.GetType()}")
        };
    }

    /// <summary>
    /// Go's <c>(*B)(p)</c> between pointer types whose pointees have ONE representation: the
    /// identity-preserving reinterpret (<see cref="PointerExtensions.Reinterpret{T,TDst}"/>, aliasing
    /// the source's slot -- `*int` as `*integer`, `*MyBuffer` as `*bytes.Buffer`), a defined pointer
    /// type's wrap or unwrap of the same box, or an ARRAY pointee re-typed through its header (the
    /// elements alias; `*[0]byte` as `*MyBytesArray0`). Anything else -- a pointee the managed model
    /// cannot alias (a defined pointer TYPE as the pointee, `**uintptr` as `*T`) -- answers false,
    /// and the caller refuses rather than copying.
    /// </summary>
    public static bool TryConvertPointer(object? box, Type dstPointerType, out object? result)
    {
        result = null;
        Type dstBoxType = underlyingPointerType(dstPointerType);
        Type? dstPointee = boxPointeeType(dstBoxType);

        if (dstPointee is null)
            return false;

        // A defined pointer SOURCE (`MyBytesArrayPtr0`) is a generated class around the box it names.
        object? src = box;

        if (src is not null && !src.GetType().IsGenericType && TryUnwrapWrapperValue(src, out object? innerBox))
            src = innerBox;

        // Go's nil converts to the destination's nil, whatever the pointee -- the caller minted it.
        if (src is null || src is INilPointer { IsNilPointer: true })
            return false;

        Type? srcPointee = boxPointeeType(src.GetType());

        if (srcPointee is null)
            return false;

        object? converted;

        if (srcPointee == dstPointee)
        {
            converted = src;
        }
        else if (s_pointerReinterpreters.GetOrAdd((srcPointee, dstPointee), static key =>
                 {
                     (Type from, Type to) = key;
                     bool representable = (bool)typeof(PointerExtensions.ReinterpretAliasesStorage<,>)
                         .MakeGenericType(from, to).GetField("Value", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

                     if (!representable)
                         return null;

                     MethodInfo reinterpret = typeof(PointerExtensions).GetMethod(nameof(PointerExtensions.Reinterpret), BindingFlags.Public | BindingFlags.Static)!
                         .MakeGenericMethod(from, to);

                     return b => reinterpret.Invoke(null, [b])!;
                 }) is { } reinterpreter)
        {
            converted = reinterpreter(src);
        }
        else if (KindOf(srcPointee) == Array && KindOf(dstPointee) == Array &&
                 (wrapperUnderlyingType(srcPointee) ?? srcPointee) == (wrapperUnderlyingType(dstPointee) ?? dstPointee))
        {
            // The same array under two Go names: re-type the HEADER (the elements stay shared through
            // its backing) into the destination pointee and box it.
            object? header = ReadPointerSlot(src);

            // A defined array pointee installs its holder on first use (a zero `new(MyBytesArray0)` starts
            // with none): touch Length on the copy read from the slot and write that copy back, so the
            // box and every later copy share ONE holder -- the elements then alias through it.
            if (header is IArray lazy && !header.GetType().IsGenericType)
            {
                _ = lazy.Length;
                WritePointerSlot(src, header);
            }

            if (header is null || !TryConvertTo(header, dstPointee, out object? retyped) || retyped is null)
                return false;

            converted = NewPointerBox(dstPointee, retyped);
        }
        else
        {
            return false;
        }

        result = wrapPointerBox(converted, dstPointerType);
        return true;
    }

    // The ж<X> a defined pointer type wraps (its generated single-argument constructor's parameter),
    // or the type itself for a plain ж<X>.
    private static Type underlyingPointerType(Type pointerType)
    {
        if (pointerType.IsGenericType)
            return pointerType;

        return wrapperConstructorOf(pointerType)?.GetParameters()[0].ParameterType ?? pointerType;
    }

    // The single field a generated named-type wrapper carries (`type MyBytesArray0 [0]byte` ->
    // array<byte>), or null for a type that is not a wrapper.
    private static Type? wrapperUnderlyingType(Type t)
    {
        return t.IsGenericType ? null : wrapperConstructorOf(t)?.GetParameters()[0].ParameterType;
    }

    // The pointee of a box type: walk the base chain to ж<T> (StandardBox<T>, FieldRefBox<T>, ...).
    private static Type? boxPointeeType(Type boxType)
    {
        for (Type? t = boxType; t is not null; t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ж<>))
                return t.GetGenericArguments()[0];
        }

        return null;
    }

    // A defined POINTER destination is the generated class around the box.
    private static object wrapPointerBox(object box, Type pointerType)
    {
        if (pointerType.IsGenericType || pointerType.IsInstanceOfType(box))
            return box;

        return TryConvertTo(box, pointerType, out object? named) && named is not null
            ? named
            : throw new InvalidOperationException($"cannot wrap {box.GetType()} as the defined pointer type {pointerType}");
    }
}
