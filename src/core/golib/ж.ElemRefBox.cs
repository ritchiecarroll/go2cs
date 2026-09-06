// ж.ElemRefBox.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Runtime.CompilerServices;
using go.golib;

namespace go;

/// <summary>
/// The ARRAY/SLICE-ELEMENT reference kind — a pointer to an element of managed collection storage
/// (Go's <c>&amp;s[i]</c> / <c>&amp;arr[i]</c>). One of the four kinds of <see cref="ж{T}"/>
/// under the B1 per-kind split, in the ratified two-slots-collapsed shape
/// (<c>docs/phase4/DESIGN-zh-box-b1.md</c> §5, N3's resolution, pre-gate benched).
/// </summary>
/// <remarks>
/// <para>
/// CONSTRUCTION-TIME canonicalization: the constructor classifies the source through the same
/// five arms the old per-call <c>CanonicalElement</c> walked, and collapses the three arms whose
/// indexers ARE the canonical access — a backed <see cref="slice{T}"/>, a named
/// <see cref="ISlice{T}"/> view's unwrapped shared header, and an <see cref="array{T}"/> alias
/// window — into the FAST pair (<see cref="m_backing"/>, absolute <see cref="m_index"/>), which
/// is canonical AND deref-equivalent BY THE INDEXER DEFINITIONS. The arms that cannot prove
/// deref-equivalence (a <see cref="PinnedBuffer"/>, a null-backing slice, a foreign
/// <see cref="IArray"/> — whose <c>Source</c> may materialize a copy) keep the ORIGINAL
/// collection in <see cref="m_foreign"/>: deref goes through its own indexer exactly as before
/// the split, and identity canonicalizes per call exactly as before the split. Nothing is traded
/// silently — the <c>&amp;StringData</c> equality contract and the element identity win coexist.
/// </para>
/// <para>
/// Identity ops on the fast arm read the stored pair directly — the construction-time
/// canonicalization that halved the token/hash workloads in the design's §2.2 measurement.
/// </para>
/// </remarks>
public sealed class ElemRefBox<T> : ж<T>
{
    // The FAST arm: canonical backing + ABSOLUTE index, deref-equivalent by the indexer
    // definitions of the three arms that populate it. Null when the foreign arm is in effect.
    private readonly T[]? m_backing;

    // The FOREIGN arm: the ORIGINAL collection, deref'd through its own indexer as before the
    // split; identity ops canonicalize per call. Null when the fast arm is in effect.
    private readonly IArray? m_foreign;

    private readonly nint m_index;

    // Create a new indexed reference into existing heap allocated collection storage. The fast
    // arm is taken ONLY for the three arms whose indexers ARE the canonical access (the ratified
    // §5 table) — a PinnedBuffer or a foreign IArray stays on the original-deref arm even when
    // its canonical storage happens to be a T[], because deref-equivalence is unproven there.
    internal ElemRefBox(IArray array, int index)
    {
        switch (array)
        {
            case slice<T> slice when slice.m_array is not null:
                m_backing = slice.m_array;
                m_index = slice.Low + index;
                break;

            case ISlice<T> view when view.Slice((nint)0, view.Length) is slice<T> shared && shared.m_array is not null:
                m_backing = shared.m_array;
                m_index = shared.Low + index;
                break;

            case array<T> arr when arr.Source is not null:
                m_backing = arr.Source;
                m_index = arr.Low + index;
                break;

            default:
                m_foreign = array;
                m_index = index;
                break;
        }

        // The box only — the collection is the caller's, already charged when it was created.
        // Leaf-ctor counting per the B1 split — same charge as before it.
        AllocationCounter.Count();
    }

    /// <inheritdoc/>
    public override ref T Value
    {
        get
        {
            if (m_backing is not null)
                return ref m_backing[m_index];

            if (m_foreign is IArray<T> typedArray)
                return ref typedArray[(int)m_index];

            throw new InvalidOperationException("Cannot get reference to value, source is not a valid array or slice reference.");
        }
    }

    /// <inheritdoc/>
    public override ref T ValueSlot
    {
        get
        {
            if (m_backing is not null)
                return ref m_backing[m_index];

            if (m_foreign is IArray<T> typedArray)
                return ref typedArray[(int)m_index];

            throw new InvalidOperationException("Cannot get reference to value, source is not a valid array or slice reference.");
        }
    }

    /// <inheritdoc/>
    // Canonical backing identity in the high bits with the ABSOLUTE element index below, so
    // same-storage element pointers order by index exactly like Go addresses.
    public override nuint PointerOrderToken
    {
        get
        {
            (object storage, nint element) = CanonicalPair();
            return unchecked(AllocationBase(RuntimeHelpers.GetHashCode(storage)) + (nuint)(uint)element);
        }
    }

    /// <inheritdoc/>
    public override bool Equals(ж<T>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (other is not ElemRefBox<T> er)
            return false;

        (object storage1, nint element1) = CanonicalPair();
        (object storage2, nint element2) = er.CanonicalPair();

        return ReferenceEquals(storage1, storage2) && element1 == element2;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        (object storage, nint element) = CanonicalPair();
        return System.HashCode.Combine(RuntimeHelpers.GetHashCode(storage), element);
    }

    /// <inheritdoc/>
    // An element reference's storage is the canonical backing array — the same resolution
    // equality and the order token use, so two pointers to one element pin one object.
    public override object? PinnableStorage => CanonicalPair().storage;

    /// <inheritdoc/>
    // An element reference names a real address into its backing array, exactly as a field
    // reference does. Its canonical storage is expected to be non-null on every reachable path,
    // so the Unpinnable arm should be unreachable here — written the same way as FieldRefBox
    // anyway, because "expected non-null" is an argument and the shape is the measurement.
    public override PointerStorage StorageKind =>
        PinnableStorage is null ? PointerStorage.Unpinnable : PointerStorage.Pinnable;

    /// <inheritdoc/>
    // The referent is the canonical backing storage (so `Ꮡ(buf, 0)`'s throwaway box resolves to
    // buf's own array).
    public override object ReferentObject => CanonicalPair().storage;

    /// <inheritdoc/>
    // The fast arm collapsed its source to (backing, absolute index), so it re-mints a whole-array
    // view here — Low is 0, making the stored absolute index valid relative to the view. Both
    // consumers depend on an answer: unsafe.Add re-mints an element box from this pair (a null
    // would turn pointer arithmetic into a nil pointer), and PrintPointer probes it to display an
    // out-of-range element reference (one-past-the-end, empty backing) without dereferencing.
    internal override (IArray, int)? ArrayRef =>
        m_foreign is not null ? (m_foreign, (int)m_index) :
        m_backing is not null ? (new array<T>(m_backing), (int)m_index) : null;

    /// <inheritdoc/>
    internal override bool TryGetElementStorage(out T[]? backing, out nint index)
    {
        if (m_backing is not null)
        {
            backing = m_backing;
            index = m_index;
            return true;
        }

        // The foreign arm re-asks per call, exactly as the pre-split walk did — a foreign
        // collection's canonical storage can still be a real T[] behind a PinnedBuffer target.
        (object storage, nint absoluteIndex) = CanonicalPair();

        if (storage is T[] typed && absoluteIndex >= 0 && absoluteIndex <= typed.Length)
        {
            backing = typed;
            index = absoluteIndex;
            return true;
        }

        backing = null;
        index = 0;
        return false;
    }

    /// <inheritdoc/>
    internal override bool TryGetElementWindow(int length, out slice<T> window)
    {
        window = default;

        if (length < 0)
            return false;

        if (!TryGetElementStorage(out T[]? backing, out nint absoluteIndex))
            return false;

        // Only REAL managed element storage can be aliased; bounds exactly as before the split.
        if (absoluteIndex < 0 || absoluteIndex + length > backing!.Length)
            return false;

        window = new slice<T>(backing, absoluteIndex, absoluteIndex + length, absoluteIndex + length);

        return true;
    }

    /// <inheritdoc/>
    internal override unsafe ж<TDst>? TryPinnedReinterpret<TDst>()
    {
        if (!TryGetElementStorage(out T[]? backing, out nint absoluteIndex) ||
            absoluteIndex < 0 || absoluteIndex >= backing!.Length)
        {
            return null;
        }

        PinnedBuffer? pin = PinnedBuffer.PinOnly(backing);

        if (pin is null)
            return null;

        // The backing cannot move from here on, so these addresses are the addresses it KEEPS.
        // They must name the same byte, or the object pinned is not the one the referent lives
        // inside and the pin guarantees nothing about it.
        void* referent = Unsafe.AsPointer(ref ValueSlot);

        if (referent != Unsafe.AsPointer(ref backing[absoluteIndex]))
        {
            pin.Dispose();
            return null;
        }

        // §4's uniform retention: a native box minted from managed storage retains its source.
        return new NativeBox<TDst>((nuint)referent, pin, retainedSource: this);
    }

    // The canonical (storage, absolute index) pair: the stored fast pair when the constructor
    // proved deref-equivalence, the per-call walk otherwise — today's cost on today's arms.
    private (object storage, nint element) CanonicalPair()
    {
        return m_backing is not null ? (m_backing, m_index) : Canonical(m_foreign!, (int)m_index);
    }

    // The five-arm canonical resolution, verbatim from the pre-split box: a re-sliced view or an
    // array<T>.Alias window names the same absolute element Go's pointer would.
    private static (object storage, nint index) Canonical(IArray array, int index)
    {
        switch (array)
        {
            case PinnedBuffer pinned:
                return (pinned.PinnedTarget ?? array, index);

            case slice<T> slice:
                return slice.m_array is null ? (array, index) : (slice.m_array, slice.Low + index);

            // A NAMED slice type wraps a slice<T> window it does not expose directly; its
            // full-window interface sub-slice hands back the shared header.
            case ISlice<T> view when view.Slice((nint)0, view.Length) is slice<T> shared && shared.m_array is not null:
                return (shared.m_array, shared.Low + index);

            // An array<T> is a struct over a shared T[] — normally the WHOLE of it, but Go's
            // slice-to-array-POINTER conversion (`(*[N]T)(s)`, array<T>.Alias) makes it a WINDOW,
            // and an element pointer taken through that window must name the same absolute
            // element as one taken through the slice it aliases.
            case array<T> arr when arr.Source is not null:
                return (arr.Source, arr.Low + index);

            default:
                return (array.Source ?? (object)array, index);
        }
    }
}
