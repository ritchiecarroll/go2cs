// MapExtensions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;

namespace go;

/// <summary>
/// C#-side views of a Go map, for C# code that calls converted Go. The converter never emits these.
/// </summary>
public static class MapExtensions
{
    /// <summary>
    /// Returns a read-only, live <see cref="IReadOnlySet{T}"/> view of a Go map's keys, the C# face of
    /// Go's set idiom <c>map[K]struct{}</c>. A nil map is the empty set. It binds a plain
    /// <see cref="map{TKey, TValue}"/> and a NAMED Go map type alike
    /// (<c>type HashSet[T comparable] map[T]struct{}</c>). See <see cref="MapKeySet{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of the map's keys.</typeparam>
    /// <typeparam name="TValue">Type of the map's values, which the view ignores.</typeparam>
    /// <param name="map">The map.</param>
    /// <returns>The key-set view.</returns>
    public static IReadOnlySet<TKey> AsReadOnlySet<TKey, TValue>(this IMap<TKey, TValue> map) where TKey : notnull
    {
        return new MapKeySet<TKey, TValue>(map);
    }
}
