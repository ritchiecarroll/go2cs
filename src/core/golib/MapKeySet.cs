// MapKeySet.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections;
using System.Collections.Generic;

namespace go;

/// <summary>
/// A read-only <see cref="IReadOnlySet{T}"/> view of a Go map's KEYS, the C# face of Go's set idiom
/// <c>map[K]struct{}</c>. <see cref="MapExtensions.AsReadOnlySet{TKey, TValue}(IMap{TKey, TValue})"/>
/// returns one.
/// </summary>
/// <remarks>
/// <para>
/// A LIVE view, not a copy: it reads the map it was made from, so a later write to that map shows
/// through it, as <see cref="Dictionary{TKey, TValue}.Keys"/> does. It never writes.
/// </para>
/// <para>
/// Every question is asked with Go's map semantics, because the map is what answers it. A nil map is
/// the empty set. Membership is Go's key equality: <c>+0</c> and <c>-0</c> are one key, and a NaN key
/// is never found, so a set holding NaN is not a subset even of itself. A key whose dynamic type is
/// not comparable panics, as the Go lookup does. The set relations read the OTHER sequence the same
/// way, through a Go map built from it, so both sides of a comparison follow one equality. Iteration
/// is Go's <c>range</c> over the map: every key once, the nil key included, in Go's unspecified order.
/// </para>
/// <para>
/// This is a view TYPE rather than an interface on the map itself on purpose. A Go map is already an
/// <see cref="IEnumerable{T}"/> of its key/value pairs; a second <see cref="IEnumerable{T}"/> (of keys)
/// on the same type leaves LINQ unable to infer its element type (CS0411 on <c>m.Count(...)</c>,
/// measured), so the set surface lives on a separate object and the map's own surface is unchanged.
/// </para>
/// </remarks>
/// <typeparam name="TKey">Type of the map's keys, the set's elements.</typeparam>
/// <typeparam name="TValue">Type of the map's values, which the view ignores.</typeparam>
public sealed class MapKeySet<TKey, TValue> : IReadOnlySet<TKey> where TKey : notnull
{
    private readonly IMap<TKey, TValue> m_map;

    /// <summary>
    /// Creates a read-only key-set view over <paramref name="map"/>.
    /// </summary>
    /// <param name="map">The map whose keys the view reads.</param>
    public MapKeySet(IMap<TKey, TValue> map)
    {
        m_map = map ?? throw new ArgumentNullException(nameof(map));
    }

    /// <summary>
    /// Gets the number of keys in the map: Go's <c>len(m)</c>.
    /// </summary>
    public int Count => m_map.Count;

    /// <summary>
    /// Reports whether the map holds <paramref name="item"/> as a key: Go's <c>_, ok := m[item]</c>.
    /// </summary>
    /// <param name="item">The key to look for.</param>
    /// <returns><c>true</c> when the map holds the key; otherwise, <c>false</c>.</returns>
    public bool Contains(TKey item)
    {
        return m_map.ContainsKey(item);
    }

    /// <inheritdoc />
    public bool IsSubsetOf(IEnumerable<TKey> other)
    {
        map<TKey, EmptyStruct> others = keysOf(other);

        return Count <= others.Count && allIn(others);
    }

    /// <inheritdoc />
    public bool IsProperSubsetOf(IEnumerable<TKey> other)
    {
        map<TKey, EmptyStruct> others = keysOf(other);

        return Count < others.Count && allIn(others);
    }

    /// <inheritdoc />
    public bool IsSupersetOf(IEnumerable<TKey> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (TKey key in other)
        {
            if (!Contains(key))
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool IsProperSupersetOf(IEnumerable<TKey> other)
    {
        map<TKey, EmptyStruct> others = keysOf(other);

        return others.Count < Count && allContained(others);
    }

    /// <inheritdoc />
    public bool Overlaps(IEnumerable<TKey> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (TKey key in other)
        {
            if (Contains(key))
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool SetEquals(IEnumerable<TKey> other)
    {
        map<TKey, EmptyStruct> others = keysOf(other);

        return others.Count == Count && allContained(others);
    }

    /// <summary>
    /// Enumerates the map's keys in Go's <c>range</c> order.
    /// </summary>
    /// <returns>An enumerator over the keys.</returns>
    public IEnumerator<TKey> GetEnumerator()
    {
        foreach (KeyValuePair<TKey, TValue> entry in m_map)
            yield return entry.Key;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // The other sequence as a Go map, so its duplicates collapse under the same equality the view's
    // own map uses (the BCL contract: set relations ignore duplicates in the other sequence).
    private static map<TKey, EmptyStruct> keysOf(IEnumerable<TKey> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        map<TKey, EmptyStruct> keys = new();

        foreach (TKey key in other)
            keys[key] = default;

        return keys;
    }

    // Every key of this view is a key of others.
    private bool allIn(map<TKey, EmptyStruct> others)
    {
        foreach (TKey key in this)
        {
            if (!others.ContainsKey(key))
                return false;
        }

        return true;
    }

    // Every key of others is a key of this view.
    private bool allContained(map<TKey, EmptyStruct> others)
    {
        foreach (KeyValuePair<TKey, EmptyStruct> entry in (IEnumerable<KeyValuePair<TKey, EmptyStruct>>)others)
        {
            if (!Contains(entry.Key))
                return false;
        }

        return true;
    }
}
