// SliceBuilder.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go;

/// <summary>
/// Builds a <see cref="slice{T}"/> from a C# collection expression: Go's <c>[]int{1, 2, 3}</c> where C# knows the
/// target type is <c>[1, 2, 3]</c> (docs/PLAN-collection-expressions.md, ruled 2026-11-05).
/// </summary>
/// <remarks>
/// <para>
/// The elements are copied into a fresh backing the slice owns, as Go's composite literal allocates one. The copy is
/// not charged to <see cref="AllocationCounter"/>, exactly as <c>slice&lt;T&gt;([1, 2, 3])</c>'s array is not: that
/// array is the C# compiler's own <c>newarr</c>, and the counter charges golib's sites only, so the two spellings of
/// one Go literal count the same.
/// </para>
/// <para>
/// An empty <c>[]</c> is Go's <c>[]T{}</c>: a non-nil slice of length 0, over the shared empty array, as
/// <c>slice&lt;T&gt;([])</c> is. Go's nil slice stays <c>default</c> (<c>nil</c>).
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class SliceBuilder
{
    /// <summary>Builds the slice a collection expression targeting <see cref="slice{T}"/> spells.</summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="elements">The expression's elements, in order.</param>
    public static slice<T> Create<T>(ReadOnlySpan<T> elements)
    {
        return new slice<T>(elements.ToArray());
    }
}
