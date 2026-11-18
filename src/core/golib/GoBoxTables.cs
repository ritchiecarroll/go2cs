// GoBoxTables.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// The lookup tables golib's generic pointer boxes share, declared once for every type argument.
/// </summary>
/// <remarks>
/// <para>
/// A static field of a generic class is new storage per type argument, and when the field's type names the type
/// parameter it is also a new instantiation of the table's whole method set: entry, container, callback and enumerator
/// types, and the collection interfaces over its key-value pair. Native AOT compiles that set, under its own symbol
/// names, for every pointee type a program reaches; on the converted TypeScript demo these tables were 30% of the
/// image's symbol names with the pointer box's generic cycle already broken. Declared here once, they are one
/// instantiation each.
/// </para>
/// <para>
/// One table holds every type argument's entries without a collision: the weak tables compare keys by reference, and
/// an entry's key IS the box, the accessor or the array it belongs to. The one keyed by value carries the pointee type
/// in its key. A value is read back as the type that stored it. Guarded by <c>PointerBoxStaticTableTests</c>: no golib
/// static table is declared closed over its type's own parameter.
/// </para>
/// </remarks>
internal static class GoBoxTables
{
    /// <summary>A box's field-view list, for the box kinds that carry no slot of their own.</summary>
    internal static readonly ConditionalWeakTable<object, ViewTable> Views = new();

    /// <summary>A typed field accessor's untyped wrapper (<c>ж&lt;T&gt;.FieldRefWrappers&lt;TElem&gt;</c>).</summary>
    internal static readonly ConditionalWeakTable<Delegate, Delegate> FieldRefWrappers = new();

    /// <summary>A backing array's persistent element boxes, in chunks (<c>ElemRefBox&lt;T&gt;</c>).</summary>
    internal static readonly ConditionalWeakTable<Array, object> ArrayElementBoxes = new();

    /// <summary>The canonical typed nil for a pointer to a Go array, per (pointee type, dims).</summary>
    internal static readonly ConcurrentDictionary<(Type, GoArrayDimsKey), object> NilArrayBoxes = new();

    /// <summary>A field accessor's Go offset (<c>FieldRefBox&lt;T&gt;</c>).</summary>
    internal static readonly ConditionalWeakTable<Delegate, FieldGoOffset> GoOffsetsByAccessor = new();

    /// <summary>A field accessor's displacement (<c>FieldRefBox&lt;T&gt;</c>).</summary>
    internal static readonly ConditionalWeakTable<Delegate, FieldDisplacement> DisplacementsByAccessor = new();

    /// <summary>The root a native-backed slice's anchor array keeps (<c>NativeSliceAnchors&lt;T&gt;</c>).</summary>
    internal static readonly ConditionalWeakTable<Array, object> SliceRoots = new();

    /// <summary>A field accessor's cached Go offset and the source type it was resolved for. Nothing in it depends on
    /// the box's type argument.</summary>
    internal sealed record FieldGoOffset(Type SourceType, nint Value);

    /// <summary>A field accessor's cached displacement and the source type it was resolved for.</summary>
    internal sealed record FieldDisplacement(Type SourceType, nuint Value);
}
