// ж.NilArrayBox.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;

namespace go;

/// <summary>
/// A nil pointer that carries the dimensions of the Go ARRAY it points at.
/// </summary>
/// <remarks>
/// NON-GENERIC on purpose: the reflection bridge reads the dims from an <c>object</c> it holds
/// without knowing the pointee type — <c>GoReflect.PointeeArrayDims</c> takes <c>object?</c> — so
/// a generic accessor on <c>ж&lt;T&gt;</c> would be unreachable from the one caller that needs it.
/// </remarks>
internal interface IGoNilArrayPointer
{
    /// <summary>The Go array dimensions, outermost first.</summary>
    long[] Dims { get; }
}

/// <summary>
/// A nil <see cref="StandardBox{T}"/> that carries the Go LENGTH of the array it points at.
/// </summary>
/// <typeparam name="T">Pointee type — always a golib <c>array&lt;E&gt;</c>.</typeparam>
/// <remarks>
/// <para>
/// A Go array's length is part of its TYPE, and it is the one part the managed emission cannot
/// carry: <c>[0]byte</c> and <c>[3]byte</c> both render as <c>array&lt;byte&gt;</c>. Everywhere the
/// reflection bridge needs that length it recovers it from a live source — a VALUE reveals its own
/// length (<c>GoReflect.ArrayDimsOfValue</c>), a struct FIELD from the declaring type's zero
/// instance, a func PARAMETER from <c>GoArrayDimsAttribute</c>. A NIL POINTER has none of those:
/// there is no pointee to measure, which <c>GoReflect.PointeeArrayDims</c> says in its own words,
/// and there is no attribute slot at an expression position. So the length has to ride the
/// CONSTRUCTION, and this is the value it rides on.
/// </para>
/// <para>
/// A SUBCLASS rather than a field on <see cref="ж{T}"/> or <see cref="StandardBox{T}"/>, and that
/// is a cost decision rather than a style one: instance state on a per-box base class is a
/// corpus-wide byte cost — eight bytes on EVERY pointer box, proportional to boxes allocated per
/// path. The Go 1.23.12 standard library contains THIRTEEN nil constructions of pointer-to-array
/// type (all of them in <c>_test.go</c>; zero in production code), so those thirteen pay for this
/// and nothing else does.
/// </para>
/// <para>
/// <see cref="StandardBox{T}"/> is unsealed for exactly this kind of extension (P-F5's resolution,
/// which <c>@unsafe.Pointer</c> already uses). This is NOT a fifth pointer KIND under the B1
/// per-kind split: it is a standard-kind nil that answers one extra question, and it inherits every
/// storage and identity property of the box it derives from.
/// </para>
/// </remarks>
internal sealed class NilArrayBox<T> : StandardBox<T>, IGoNilArrayPointer
{
    private readonly long[] m_dims;

    internal NilArrayBox(long[] dims) : base(nil)
    {
        m_dims = dims;
    }

    /// <summary>The Go array dimensions, outermost first — <c>[4][8]byte</c> ⇒ <c>{4, 8}</c>.</summary>
    public long[] Dims => m_dims;
}

/// <summary>
/// Structural key for the per-<c>(T, dims)</c> intern table.
/// </summary>
/// <remarks>
/// The table is a static of the CLOSED GENERIC <c>ж&lt;T&gt;</c>, so <typeparamref name="T"/> is
/// already the outer half of the key and this carries only the dims. Equality is structural
/// because two <c>(*[3]byte)(nil)</c> expressions must yield the SAME instance — Go's typed nils
/// of one type compare equal, and the managed comparison at an <c>object</c> slot is reference
/// equality, which is the same reason <see cref="ж{T}.NilBox"/> is one canonical instance rather
/// than a fresh box per site.
/// </remarks>
internal readonly struct GoArrayDimsKey : IEquatable<GoArrayDimsKey>
{
    private readonly long[] m_dims;
    private readonly int m_hash;

    internal GoArrayDimsKey(long[] dims)
    {
        m_dims = dims;

        HashCode hash = new();

        foreach (long dim in dims)
            hash.Add(dim);

        m_hash = hash.ToHashCode();
    }

    internal long[] Dims => m_dims;

    public bool Equals(GoArrayDimsKey other)
    {
        if (ReferenceEquals(m_dims, other.m_dims))
            return true;

        if (m_dims is null || other.m_dims is null || m_dims.Length != other.m_dims.Length)
            return false;

        for (int i = 0; i < m_dims.Length; i++)
        {
            if (m_dims[i] != other.m_dims[i])
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is GoArrayDimsKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return m_hash;
    }
}

public abstract partial class ж<T>
{
    // The intern table is a static of the closed generic, so it IS per-(T, dims) with no Type in
    // the key and no table shared between pointee types.
    /// <summary>
    /// The canonical typed nil instance for a pointer to a Go ARRAY, carrying the array's
    /// dimensions — what <c>(*[N]E)(nil)</c> is.
    /// </summary>
    /// <param name="dims">The Go array dimensions, outermost first.</param>
    /// <remarks>
    /// <para>
    /// The array analog of <c>channel&lt;T&gt;.SendOnly</c>: a nil VALUE that carries the part of
    /// its Go type the managed type cannot. Interned per <c>(T, dims)</c> rather than per
    /// <typeparamref name="T"/> alone, which is <see cref="NilBox"/>'s canonical-instance property
    /// widened by one axis and not a break of it — <c>*[3]byte</c> and <c>*[0]byte</c> are
    /// DIFFERENT Go types, so Go itself requires them to compare unequal, while two
    /// <c>(*[3]byte)(nil)</c> expressions must compare equal and do.
    /// </para>
    /// <para>
    /// Dimensions are <c>long</c>, matching <see cref="GoArrayDimsAttribute"/>'s deliberate 64-bit
    /// widening rather than <c>GoMapKeyDimsAttribute</c>'s un-widened form. A Go array length is
    /// Go's <c>int</c>, and the standard library uses the full range: <c>runtime/vdso_linux.go</c>
    /// declares <c>*[1&lt;&lt;50 - 1]byte</c>, Go's pointer-to-unbounded-array idiom. That
    /// declaration reaches the bridge as a FIELD today; it reaches THIS method the first time
    /// anyone writes it as a conversion, and a 32-bit carrier could not hold it.
    /// </para>
    /// </remarks>
    public static ж<T> NilBoxOfDims(params long[] dims)
    {
        // No dimensions is not this method's shape — it is the plain typed nil, and answering it
        // here rather than minting a dims-less NilArrayBox keeps ONE instance for that case.
        if (dims is not { Length: > 0 })
            return NilBox;

        return (ж<T>)GoBoxTables.NilArrayBoxes.GetOrAdd((typeof(T), new GoArrayDimsKey(dims)), static key => new NilArrayBox<T>(key.Item2.Dims));
    }
}
