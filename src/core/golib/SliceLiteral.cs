// SliceLiteral.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// The elements of a Go slice literal written as a C# collection expression: the converter writes Go's
/// <c>[]int{1, 2, 3}</c> as <c>slice&lt;nint&gt;([1, 2, 3])</c>, and the collection expression binds
/// <see cref="builtin.slice{T}(SliceLiteral{T})"/> through this type.
/// </summary>
/// <remarks>
/// <para>
/// A collection expression cannot target <see cref="slice{T}"/> itself: C# takes a collection's element type from
/// its public enumerator, and slice&lt;T&gt;'s yields Go's range pair <c>(nint, T)</c>. This type's enumerator
/// yields <typeparamref name="T"/>, so it can carry the literal's elements to the builtin.
/// </para>
/// <para>
/// No other argument converts to this type, so the builtin overload that takes it binds only a collection
/// expression and never changes how an existing <c>slice&lt;T&gt;(…)</c> call resolves. The copy into a fresh
/// backing array is not charged to <see cref="AllocationCounter"/>, exactly like the <c>new T[]{…}</c> array the
/// earlier form allocated, so <c>testing.AllocsPerRun</c> reads the same either way.
/// </para>
/// </remarks>
[CollectionBuilder(typeof(SliceLiteral), nameof(SliceLiteral.Create))]
public readonly struct SliceLiteral<T> : IEnumerable<T>
{
    internal readonly T[] Items;

    internal SliceLiteral(T[] items) => Items = items;

    /// <summary>Enumerates the literal's elements.</summary>
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(Items ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>The collection-expression builder of <see cref="SliceLiteral{T}"/>.</summary>
public static class SliceLiteral
{
    /// <summary>
    /// Copies <paramref name="items"/> into a fresh backing array, uncounted. An empty literal gets a non-null
    /// zero-length backing, so the slice built from it is empty and not nil, as Go's <c>[]T{}</c> is.
    /// </summary>
    public static SliceLiteral<T> Create<T>(ReadOnlySpan<T> items) => new(items.ToArray());
}
