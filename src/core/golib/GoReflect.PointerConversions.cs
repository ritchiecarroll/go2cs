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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        // The slice's own face (trim stage 3c-1: ISlice<T>'s default member, compiled for every slice type the program
        // has), for exactly the values the generic helper took: a slice of the pointee's element type.
        if (source is not IGoReflectSlice sourceSlice || ((IGoReflectSequence)source).ReflectElementType != elem)
            throw new InvalidOperationException($"AliasSliceAsArrayPointer: unsupported slice {source.GetType()}");

        object aliased = sourceSlice.ReflectAliasAsArray(length);

        // A defined array pointee (`type MyBytesArray0 [0]byte`) takes the aliased HEADER through its
        // generated single-argument constructor: the header's backing reference is the alias.
        object pointeeValue = aliased;

        if (arrayType != pointee && !TryConvertTo(aliased, pointee, out pointeeValue!))
            throw new InvalidOperationException($"AliasSliceAsArrayPointer: cannot wrap {arrayType} as {pointee}");

        return wrapPointerBox(NewPointerBox(pointee, pointeeValue), pointerType);
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

    /// <summary>
    /// A COPY of <paramref name="src"/> re-typed as <paramref name="dstType"/> when the two value types have
    /// ONE representation -- Go's value conversion between struct types identical up to their tags
    /// (`struct{ x int "a" }` to `struct{ x int "b" }`, two lifted C# structs of one shape). False for
    /// anything else; the caller refuses rather than guessing.
    /// </summary>
    /// <remarks>
    /// Trim stage 3c-2a: the copy is FIELD-WISE over the field metadata the type registry keeps, not a byte
    /// reinterpret closed over the pair with MakeGenericMethod (which Native AOT cannot). Never a raw byte copy:
    /// these structs carry managed references (a string, a slice's backing, a pointer), and a raw copy would bypass
    /// the GC's write barriers. Go's conversion IS a copy (classified 2026-10-09: a write through either value is
    /// not seen through the other, in Go and here), so a field-wise one is the same value. The shapes it admits are
    /// PointerExtensions' LayoutCompatible relation, in its own order: one side a (possibly nested) single-field
    /// wrapper over the other, or the same fields in the same order, each the same type or itself compatible. The
    /// old gate also admitted two reference-free structs of any shape by size alone, a byte pun reflect never needs:
    /// it calls this only for Go structs of identical underlying type, whose lifted fields correspond one to one.
    /// </remarks>
    public static bool TryReinterpretValue(object src, Type dstType, out object? result)
    {
        Type srcType = src.GetType();

        if (srcType == dstType)
        {
            result = src;
            return true;
        }

        return tryCopyFieldWise(src, srcType, dstType, out result);
    }

    private static bool tryCopyFieldWise(object src, Type from, Type to, out object? result)
    {
        result = null;

        if (from == to)
        {
            result = src;
            return true;
        }

        if (!from.IsValueType || !to.IsValueType || from.IsPrimitive || to.IsPrimitive || from.IsEnum || to.IsEnum)
            return false;

        // One side a single-field wrapper over the other (the generated defined-type-over-struct wrapper, the
        // embedding idiom): unwrap the source down to the destination, or wrap the source up to it.
        if (singleFieldUnwrapped(from) == to)
        {
            object inner = src;

            for (Type type = from; type != to; type = GoFieldMetadata.InstanceFields(type)[0].FieldType)
                inner = GoFieldMetadata.InstanceFields(type)[0].GetValue(inner)!;

            result = inner;
            return true;
        }

        if (singleFieldUnwrapped(to) == from)
        {
            result = wrapFieldWise(src, from, to);
            return true;
        }

        // The same fields in the same order, all the way down.
        FieldInfo[] fromFields = GoFieldMetadata.InstanceFields(from), toFields = GoFieldMetadata.InstanceFields(to);

        if (fromFields.Length == 0 || fromFields.Length != toFields.Length)
            return false;

        object destination = uninitializedValue(to);

        for (int i = 0; i < fromFields.Length; i++)
        {
            object? value = fromFields[i].GetValue(src);

            if (fromFields[i].FieldType != toFields[i].FieldType)
            {
                // A differing field must be a compatible value type; a reference field of another type never is.
                if (value is null || !tryCopyFieldWise(value, fromFields[i].FieldType, toFields[i].FieldType, out value))
                    return false;
            }

            toFields[i].SetValue(destination, value);
        }

        result = destination;
        return true;
    }

    // `to` holding `src` at the bottom of its single-field chain.
    private static object wrapFieldWise(object src, Type from, Type to)
    {
        if (to == from)
            return src;

        FieldInfo field = GoFieldMetadata.InstanceFields(to)[0];
        object wrapper = uninitializedValue(to);

        field.SetValue(wrapper, wrapFieldWise(src, from, field.FieldType));

        return wrapper;
    }

    // The type a chain of single-field value types bottoms out at (PointerExtensions' UnwrapSingleField).
    private static Type singleFieldUnwrapped(Type type)
    {
        // Bounded: each hop strictly descends into a field's type, and a struct cannot contain itself.
        while (type.IsValueType && !type.IsPrimitive && !type.IsEnum && GoFieldMetadata.InstanceFields(type) is [var only])
            type = only.FieldType;

        return type;
    }

    // A value type's zero, its fields then stored one by one (trim stage 3c-2a).
    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification =
        "No constructor runs: the zero value's fields are all stored by the field-wise copy. The types are Go structs " +
        "the program converts between, which it has constructed, and whose fields the type registry keeps.")]
    private static object uninitializedValue(Type type) => RuntimeHelpers.GetUninitializedObject(type);

    /// <summary>
    /// The same channel (same core -- the queue, its lock, its waiters) re-stamped with
    /// <paramref name="cargo"/>: <c>reflect.Value.Convert</c> between channel types hands out a value
    /// that DESCRIBES itself with the destination's direction (`chan<- int` from a `chan int`), which
    /// on this bridge is cargo on the channel VALUE (<see cref="channel{T}.WithCargo"/>). A value that
    /// is not a <c>channel&lt;T&gt;</c> is returned as it is.
    /// </summary>
    public static object WithChanCargo(object channel, ChanCargo? cargo)
    {
        // channel<T>'s own face (trim stage 3c-1), which only channel<T> implements: the values the generic helper took.
        return channel is IGoReflectChannel restampable ? restampable.ReflectWithCargo(cargo) : channel;
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
