// ElementAddress.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// The identity of a slice or array ELEMENT's address, as a value: what Go's <c>&amp;x[i] == &amp;y[j]</c>
/// compares, without the <see cref="ж{T}"/> box golib's <c>Ꮡ(x, i)</c> mints for it.
/// </summary>
/// <remarks>
/// <para>
/// Go's <c>&amp;x[i]</c> is a plain interior pointer and costs nothing, and math/big's <c>alias</c>, the crypto alias
/// packages' <c>InexactOverlap</c>, regexp, cryptobyte and syscall compare two of them on hot paths. The converter
/// emits each operand of such a compare as <c>Ꮡelem(x, i)</c> (builtin.cs) or <c>p.Ꮡelem&lt;E&gt;(i)</c> through a
/// pointer to an array (ж.cs), and this struct is what those return: the element's backing storage and absolute
/// index, nothing allocated (1.26.10.2 seat #10, docs/phase4/DESIGN-element-address-compare.md).
/// </para>
/// <para>
/// It answers exactly what <see cref="ElemRefBox{T}"/>'s <c>Equals</c> answers, in the same order: every
/// zerobase element is one address; a NATIVE element is its address; a managed element is its canonical
/// (backing, absolute index) pair. The one difference is stated: <c>Ꮡ</c> mints a <see cref="NativeBox{T}"/> for a
/// native SLICE element, which compares false against an element box over the same address taken through a
/// native array view, where two <see cref="ElementAddress{T}"/> values over one address compare true, which is
/// Go's answer.
/// </para>
/// <para>
/// <b>There is no conversion to or from <see cref="ж{T}"/>, ever.</b> Only <c>==</c> and <c>!=</c> use this
/// type: the converter rewrites a compare only when BOTH operands are element address-ofs, so an
/// <see cref="ElementAddress{T}"/> never meets a box, a nil, an interface or a pointer conversion. A conversion
/// would let a mixed compare bind silently and mint the box this type exists to remove.
/// </para>
/// </remarks>
/// <typeparam name="T">Element type.</typeparam>
public readonly struct ElementAddress<T> : IEquatable<ElementAddress<T>>
{
    // The backing T[] (or the foreign collection ElemRefBox canonicalizes onto), GoZeroBase.Box for every
    // zerobase element, or null for a NATIVE element, whose m_index is then its address.
    private readonly object? m_storage;

    private readonly nint m_index;

    private ElementAddress(object? storage, nint index)
    {
        m_storage = storage;
        m_index = index;
    }

    // Every zerobase element: the shared zero-size slot and the zero-capacity slot (ж<T>.NamesZeroBase).
    internal static ElementAddress<T> Zerobase => new(GoZeroBase.Box, 0);

    internal static ElementAddress<T> Native(nuint address) => new(null, (nint)address);

    internal static ElementAddress<T> Managed(object storage, nint index) => new(storage, index);

    /// <summary>The element <paramref name="index"/> of <paramref name="target"/>, index already checked.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ElementAddress<T> OfSlice(slice<T> target, nint index)
    {
        if (target.IsNativeBacked)
        {
            unsafe
            {
                return Native(target.NativeElementAddress(index));
            }
        }

        return OfBacking(target.m_array, target.Low + index);
    }

    /// <summary>The element <paramref name="index"/> of <paramref name="target"/>, index already checked.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ElementAddress<T> OfArray(array<T> target, nint index)
    {
        // a zero-valued array header has no backing; its element box keeps the foreign arm, so ask the box
        // (an allocation on a path no compare in the corpus takes)
        if (target.Source is null)
            return new ElemRefBox<T>(target, (int)index).Identity();

        return OfBacking(target.Source, target.Low + index);
    }

    // ElemRefBox's fast arm and NamesZeroBase, without the box: every element of the shared zero-size slot and
    // the zero-capacity slot is zerobase (FastIndex folds the former's index to 0 for the same reason).
    private static ElementAddress<T> OfBacking(T[] backing, nint absoluteIndex)
    {
        if ((GoZeroSizeFacts<T>.IsZeroSize && ReferenceEquals(backing, GoZeroSizeFacts<T>.Storage)) || ReferenceEquals(backing, GoZeroCapacitySlot<T>.Backing))
            return Zerobase;

        return new ElementAddress<T>(backing, absoluteIndex);
    }

    /// <inheritdoc/>
    public bool Equals(ElementAddress<T> other) => ReferenceEquals(m_storage, other.m_storage) && m_index == other.m_index;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ElementAddress<T> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(m_storage is null ? 0 : RuntimeHelpers.GetHashCode(m_storage), m_index);

    public static bool operator ==(ElementAddress<T> left, ElementAddress<T> right) => left.Equals(right);

    public static bool operator !=(ElementAddress<T> left, ElementAddress<T> right) => !left.Equals(right);
}
