// GoBoxTables.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// EXPERIMENT (i9, B4 lever E): the four tables <c>ж&lt;T&gt;</c> used to declare once per closed generic,
/// declared here ONCE for every pointee type.
/// </summary>
/// <remarks>
/// A static field of a generic class is a new framework instantiation per type argument, and each
/// <c>ConditionalWeakTable&lt;K,V&gt;</c> or <c>ConcurrentDictionary&lt;K,V&gt;</c> instantiation brings its own
/// entry, container, callback and enumerator types plus the collection interfaces over its
/// <c>KeyValuePair&lt;K,V&gt;</c>. On a two-line program that was 9,757 weak-table instantiations and the
/// largest family in the Native AOT object file's symbol string table. Keys are compared by reference
/// in the weak tables, so one table keyed on the base type holds every pointee type's entries without
/// a collision: an entry's key IS the box, the accessor or the backing array it belongs to.
/// </remarks>
internal static class GoBoxTables
{
    /// <summary>A box's field-view list, for the box kinds that carry no slot of their own (was <c>ж&lt;T&gt;.s_views</c>).</summary>
    internal static readonly ConditionalWeakTable<object, ViewTable> Views = new();

    /// <summary>A typed field accessor's untyped wrapper (was <c>ж&lt;T&gt;.FieldRefWrappers&lt;TElem&gt;.s_wrappers</c>).</summary>
    internal static readonly ConditionalWeakTable<Delegate, Delegate> FieldRefWrappers = new();

    /// <summary>A backing array's persistent element boxes, in chunks (was <c>ElemRefBox&lt;T&gt;.s_arrayElementBoxes</c>).</summary>
    internal static readonly ConditionalWeakTable<Array, object> ArrayElementBoxes = new();

    /// <summary>The canonical typed nil for a pointer to a Go array, per (pointee type, dims) (was <c>ж&lt;T&gt;.s_nilArrayBoxes</c>).</summary>
    internal static readonly ConcurrentDictionary<(Type, GoArrayDimsKey), object> NilArrayBoxes = new();
}
