// GoReflect.InlineArrays.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using go.golib;

namespace go;

// ---------------------------------------------------------------------------------------------
// INLINE ARRAYS — a Go array value held inside its owner (docs/phase4/DESIGN-array-value-storage.md, 7.7).
//
// The family lives in go.arrays.dll (array1<T> ... array4096<T>). golib names no family type: it recognises a member by
// [GoInlineArray(N)] on its generic definition, so a program that does not opt in never loads go.arrays.dll (P7).
//
// TYPE-LEVEL reflection knows a member as a Go array of N: KindOf answers Array, ElementType answers T and the dims
// come from the type, so GoSizeOf, GoAlignOf and GoFieldOffsets derive a holder's Go layout. A [0]T field of a holder
// is a readonly alignment CARRIER ([GoAlignmentCarrier]); GoFields projects it as the Go field it stands for.
//
// VALUE-LEVEL reflection does not reach an inline field yet: reflect's array operations go through IArray, which a
// member does not implement, so Len would read 0 and Index would refuse with the wrong text. A read or an address of
// an inline field therefore panics, naming the field (7.7.4, item 2).
//
// THE GATE, InlineLayoutMismatch, proves a holder's CLR layout IS its Go layout, at every level, from four checks:
// Go's offsets are known; every CLR field offset equals Go's; the CLR size equals Go's; the CLR alignment equals Go's.
// The CLR side is the table the struct measures itself (IGoManagedLayout, written by go2cs-gen), never Marshal.OffsetOf.
// ---------------------------------------------------------------------------------------------
public static partial class GoReflect
{
    private static readonly ConcurrentDictionary<Type, int> s_inlineArrayLengths = new();

    /// <summary>
    /// The Go length <c>N</c> when <paramref name="t"/> is a member of the inline-array family
    /// (<c>go.arrays.array&lt;N&gt;&lt;T&gt;</c>), or 0 for every other type.
    /// </summary>
    public static int InlineArrayLength(Type? t)
    {
        if (t is null || !t.IsValueType || !t.IsGenericType)
            return 0;

        return s_inlineArrayLengths.GetOrAdd(t, static type =>
            type.GetGenericTypeDefinition().GetCustomAttribute<GoInlineArrayAttribute>(false)?.Length ?? 0);
    }

    // A family member's dims: its own length, then its element's when the element is itself an array.
    private static nint[] inlineArrayDims(Type t, int length)
    {
        Type element = t.GetGenericArguments()[0];

        return KindOf(element) == Array && DeclaredArrayDims(element) is { Length: > 0 } inner ? [length, .. inner] : [length];
    }

    // The Go field a [GoAlignmentCarrier] field stands for: its managed type, array<T>, of length 0.
    private static bool tryAlignmentCarrier(FieldInfo field, out Type goType, out nint[] dims)
    {
        if (field.GetCustomAttribute<GoAlignmentCarrierAttribute>(false) is { } carrier)
        {
            goType = carrier.GoType;
            Type? element = ElementType(goType);

            dims = element is not null && KindOf(element) == Array && DeclaredArrayDims(element) is { Length: > 0 } inner ? [0, .. inner] : [0];
            return true;
        }

        goType = field.FieldType;
        dims = [];
        return false;
    }

    // The panic for a value-level reflective access to an inline field (7.7.4, item 2).
    private static PanicException inlineFieldPanic(GoFieldInfo field) =>
        new((@string)$"reflect: field {field.DeclaringType?.Name}.{field.Name} is a Go array held inline (go.arrays); reflection does not read or address it");

    private static readonly ConcurrentDictionary<Type, string> s_inlineLayoutMismatch = new();

    /// <summary>
    /// Why the CLR layout of the Go struct <paramref name="t"/>, held with inline arrays, is NOT its Go layout, naming
    /// the type and the field at the first level that differs; <c>null</c> only when it is proven equal at every level.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The gate of docs/phase4/DESIGN-array-value-storage.md, 7.6.3. Unlike <see cref="ClrLayoutMismatch"/> it never
    /// answers "unproven" with <c>null</c>: a struct whose Go offsets are unknown, or that carries no managed layout
    /// table, FAILS. At every level it checks that Go's offsets are known, that every CLR field offset equals Go's, that
    /// the CLR size equals Go's and that the CLR alignment equals Go's, and it descends into every struct a field holds,
    /// through any arrays.
    /// </para>
    /// <para>
    /// Alignment is the check offsets and size cannot replace: S22 without its <c>[0]int16</c> carrier has Go's size
    /// and Go's offsets and still sits at the wrong offset inside any struct that holds it.
    /// </para>
    /// </remarks>
    public static string? InlineLayoutMismatch(Type t)
    {
        string reason = s_inlineLayoutMismatch.GetOrAdd(t, static type => inlineLayoutMismatch(type, 0) ?? "");

        return reason.Length == 0 ? null : reason;
    }

    private static string? inlineLayoutMismatch(Type t, int depth)
    {
        if (depth > MaxLayoutDepth)
            return $"{t.Name} nests deeper than {MaxLayoutDepth} levels";

        if (KindOf(t) != Struct || !t.IsValueType)
            return $"{t.Name} is not a Go struct held by value";

        if (GoFieldOffsets(t) is not { } goOffsets)
            return $"{t.Name}: Go's field offsets are unknown";

        if (managedLayoutOf(t) is not { } managed)
            return $"{t.Name} carries no managed layout table";

        GoFieldInfo[] fields = GoFields(t);

        for (int i = 0; i < fields.Length; i++)
        {
            GoFieldInfo field = fields[i];

            if (field.Path is not [var member])
                return $"{t.Name}.{field.Name} is not a field of {t.Name} itself";

            if (!managed.TryGetOffset(member.Name, out nint clrOffset))
                return $"{t.Name}.{field.Name} is missing from the managed layout table";

            if (clrOffset != goOffsets[i])
                return $"{t.Name}.{field.Name} is at CLR offset {clrOffset} where Go lays it at {goOffsets[i]}";
        }

        nint goSize = GoSizeOf(t);

        if (managed.Size != goSize)
            return $"{t.Name} is {managed.Size} bytes in the CLR where Go's size is {goSize}";

        nint goAlign = GoAlignOf(t);

        if (managed.Alignment != goAlign)
            return $"{t.Name} aligns to {managed.Alignment} in the CLR where Go aligns it to {goAlign}";

        foreach (GoFieldInfo field in fields)
        {
            Type held = field.Type;

            while (KindOf(held) == Array && ElementType(held) is { } element)
                held = element;

            if (KindOf(held) == Struct && inlineLayoutMismatch(held, depth + 1) is { } inner)
                return $"{t.Name}.{field.Name}: {inner}";
        }

        return null;
    }

    private static readonly ConcurrentDictionary<Type, GoManagedLayout?> s_managedLayouts = new();

    // The table a struct measures itself (IGoManagedLayout), read off its zero value.
    private static GoManagedLayout? managedLayoutOf(Type t) =>
        s_managedLayouts.GetOrAdd(t, static type => DefaultValueOf(type) is IGoManagedLayout measured ? measured.ManagedLayout : null);
}
