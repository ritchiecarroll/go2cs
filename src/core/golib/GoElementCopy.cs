// GoElementCopy.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using go.golib;

namespace go;

/// <summary>
/// The element moves behind Go's <c>copy</c> and <c>append</c>, which copy each element's VALUE.
/// </summary>
/// <remarks>
/// <para>
/// An element that owns array storage (an <see cref="array{T}"/>, a generated named array, a
/// <see cref="GoValueCloneAttribute"/> struct: <see cref="array{T}.ElementNeedsDeepCopy"/>) is a
/// struct over storage of its own, so a span copy of the elements moves that storage along and
/// leaves two slices holding ONE inner array: after <c>copy(dst, src)</c>, <c>src[0][1] = 9</c>
/// changed <c>dst</c> too. Every other element type is a span copy here, as before: the tests are
/// per-T constants, so the JIT keeps one memmove and nothing else.
/// </para>
/// <para>
/// Two moves, as in Go. <see cref="CopyInto{T}"/> writes into storage that already holds elements
/// (<c>copy</c>, an <c>append</c> within capacity): each element is assigned in place, so a slice
/// or pointer taken from a destination element beforehand still names it afterwards.
/// <see cref="CloneInto{T}"/> fills new storage (append's growth and its first backing): each
/// element is a copy of its own, one counted malloc per element that owns an array, where Go's
/// memmove has none.
/// </para>
/// </remarks>
internal static class GoElementCopy
{
    /// <summary>
    /// Go's <c>copy</c> of <paramref name="from"/> into <paramref name="to"/>'s existing elements,
    /// which may overlap it (Go permits overlap in copy). The lengths are equal.
    /// </summary>
    internal static void CopyInto<T>(ReadOnlySpan<T> from, Span<T> to)
    {
        if (!array<T>.ElementNeedsDeepCopy)
        {
            // A memmove, as Go's copy is: overlap transfers correctly.
            from.CopyTo(to);
            return;
        }

        // Two windows over one storage, the destination above the source (`copy(s[1:], s)`): the walk
        // runs from the top down, so no element is read after it was overwritten.
        bool topDown = from.Overlaps(to, out int offset) && offset > 0;

        if (topDown)
        {
            for (int i = from.Length - 1; i >= 0; i--)
                to[i] = CopyOf(to[i], from[i]);
        }
        else
        {
            for (int i = 0; i < from.Length; i++)
                to[i] = CopyOf(to[i], from[i]);
        }
    }

    /// <summary>
    /// Go's element copy into NEW storage: <paramref name="to"/> holds nothing anyone else can see,
    /// and must end holding no storage <paramref name="from"/> holds. The lengths are equal.
    /// </summary>
    internal static void CloneInto<T>(ReadOnlySpan<T> from, Span<T> to)
    {
        from.CopyTo(to);
        CloneElements(to);
    }

    /// <summary>
    /// Elements just copied into new storage by a span copy, each made a copy of its own where it owns
    /// array storage; nothing for any other element type.
    /// </summary>
    internal static void CloneElements<T>(Span<T> copied)
    {
        if (!array<T>.ElementNeedsDeepCopy)
            return;

        for (int i = 0; i < copied.Length; i++)
            copied[i] = Clone(copied[i]);
    }

    // One element written into an existing slot: in place where the element can take it (IGoValueAssign,
    // as ArrayExtensions.Assign walks a nested array), which keeps the slot's own storage and returns the
    // value to write back; else a copy of its own (array<T>.Clone's per-element copy).
    private static T CopyOf<T>(T slot, in T source)
    {
        if (array<T>.ElementAssignsInPlace)
            return ((IGoValueAssign<T>)slot!).AssignedFrom(source);

        return Clone(source);
    }

    // The per-element copy array<T>.Clone takes: through ICloneable, which every array kind and every
    // GoValueClone struct implements, and which recurses through deeper nestings.
    private static T Clone<T>(T element) => (T)((ICloneable)element!).Clone();
}
