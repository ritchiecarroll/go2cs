// RangePairs.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;

namespace go;

/// <summary>
/// Go's range over a slice or an array with its value, <c>for i, v := range x</c> (and <c>for _, v := range x</c>), as
/// the converter emits it: <c>foreach (var (i, v) in range(x))</c>.
/// </summary>
/// <remarks>
/// <para>
/// A slice and an array enumerate their VALUES, the element type a collection expression <c>[1, 2, 3]</c> builds from
/// (docs/PLAN-collection-expressions.md, ruled 2026-11-05), so Go's (index, value) pairs have this explicit door. Each
/// overload returns an allocation-free struct enumerable over the pairs. The operand is evaluated once, when the loop
/// starts, as Go evaluates the range expression once.
/// </para>
/// <para>
/// The index-only range, <c>for i := range x</c>, reads no element and is <c>foreach (var i in range(len(x)))</c>, the
/// integer range of <see cref="RangeOverInt{T}"/>; an array's constant length is <c>range(N)</c>, since Go does not
/// evaluate the operand there either.
/// </para>
/// <para>
/// A slice, a variadic parameter's stack slice and an array bind their own overloads by identity. A named slice or
/// array type, or a type parameter whose core type is one, binds <see cref="range{T}(IArray{T})"/>, which reads the
/// sequence through its length and indexer. None of these is ambiguous with the integer <see cref="range{T}(T)"/>: its
/// constraint rejects them.
/// </para>
/// </remarks>
public static partial class builtin
{
    /// <summary>Enumerates a slice's (index, value) pairs: Go's <c>range s</c>.</summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="s">Slice to range over.</param>
    public static slice<T>.RangeOver range<T>(slice<T> s) => new(s);

    /// <summary>Enumerates the (index, value) pairs of a variadic parameter's slice: Go's <c>range s</c>.</summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="s">The stack slice a variadic parameter is emitted as.</param>
    /// <remarks>
    /// <see cref="sslice{T}"/> is a ref struct, so no other overload can take it: without this one C# reaches the
    /// integer <see cref="range{T}(T)"/> and refuses the ref struct as its type argument (CS9244).
    /// </remarks>
    public static sslice<T>.RangeOver range<T>(sslice<T> s) => s.Range();

    /// <summary>Enumerates an array's (index, value) pairs: Go's <c>range a</c>.</summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="a">Array to range over.</param>
    public static array<T>.RangeOver range<T>(array<T> a) => new(a);

    /// <summary>
    /// Enumerates the (index, value) pairs of the array a pointer addresses: Go's <c>range p</c> over a <c>*[N]T</c>
    /// with a value variable, which reads through the pointer, so a nil one panics as Go's does.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="p">Pointer to the array.</param>
    public static array<T>.RangeOver range<T>(ж<array<T>> p) => new(p.Value);

    /// <summary>
    /// Enumerates the (index, value) pairs of a named slice or array type, or of a type parameter whose core type is
    /// one: Go's <c>range x</c>.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="x">Sequence to range over.</param>
    /// <remarks>
    /// The sequence is read through <see cref="IArray{T}"/>'s length, read once, and its indexer, so the loop sees the
    /// elements live, as Go's range over a slice does. A named type that is a struct is boxed once, when the loop
    /// starts.
    /// </remarks>
    public static SequenceRange<T> range<T>(IArray<T> x) => new(x);
}

/// <summary>
/// What <see cref="builtin.range{T}(IArray{T})"/> returns: the (index, value) pairs of a named slice or array type or of
/// a type parameter's sequence.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public readonly struct SequenceRange<T>
{
    private readonly IArray<T> m_sequence;

    internal SequenceRange(IArray<T> sequence)
    {
        m_sequence = sequence;
    }

    public Enumerator GetEnumerator() => new(m_sequence);

    /// <summary>The (index, value) enumerator <c>foreach (var (i, v) in range(x))</c> drives.</summary>
    public struct Enumerator
    {
        private readonly IArray<T>? m_sequence;
        private readonly nint m_length;
        private nint m_index;

        internal Enumerator(IArray<T>? sequence)
        {
            m_sequence = sequence;
            m_length = sequence?.Length ?? 0;
            m_index = -1;
        }

        public readonly (nint, T) Current => (m_index, m_sequence![m_index]);

        public bool MoveNext()
        {
            if (m_index >= m_length)
                return false;

            m_index++;
            return m_index < m_length;
        }
    }
}
