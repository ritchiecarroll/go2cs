// ж.NativeArrayBox.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using go.golib;

namespace go;

/// <summary>
/// A pointer to a FIXED-SIZE ARRAY that lives in NATIVE memory — Go's <c>*[N]T</c> over off-heap
/// storage, the shape <c>runtime</c>'s page allocator builds when it hangs a <c>sysAlloc</c>'d
/// chunk block off <c>p.chunks[l1]</c>.
/// </summary>
/// <remarks>
/// <para>
/// This is NOT <see cref="NativeBox{T}"/> with <c>T = array&lt;E&gt;</c>, and the difference is the
/// whole reason this kind exists. <c>array&lt;E&gt;</c> is a struct whose first field is a MANAGED
/// <c>E[]</c>, while Go's <c>*[N]T</c> points at N bare contiguous elements with no header of any
/// kind. A native box of an array therefore reinterprets the first element's bytes AS an array
/// header and hands back a garbage <c>E[]</c> reference — measured as a CLR prestub null read
/// (exit 139, blank stderr) from <c>chunkOf</c> in the page allocator. There is nothing in an
/// <c>array&lt;E&gt;</c> to rebase, which is exactly what distinguishes it from
/// <c>slice&lt;E&gt;</c>: a slice IS a rebasable header, and that is why this box answers the
/// element door with one.
/// </para>
/// <para>
/// So <see cref="Value"/> REFUSES rather than fabricating an <c>array&lt;T&gt;</c> that cannot
/// exist. Consumers reach the elements through the element door — <c>at&lt;T&gt;(i)</c>, which
/// consults <see cref="TryGetNativeArrayView{Telem}"/> before it touches <c>Value</c> — and that
/// door keeps `at`'s single implementation, single bounds check and single element-box
/// construction. An index past the minted length is refused by that same existing check, which is
/// what makes the length handed in at construction load-bearing rather than advisory.
/// </para>
/// <para>
/// The length comes from the MINTING SITE, because that is the only place that knows it: Go's
/// <c>*[N]T</c> carries N in its type, and the store that installs the block has the size in hand
/// (the page allocator's <c>l2Size</c>). Nothing downstream can recover it from the address.
/// </para>
/// </remarks>
internal sealed class NativeArrayBox<T> : ж<array<T>>
{
    // The base of the element block — never managed storage this box owns.
    private readonly nuint m_nativeAddr;

    // How many elements the block holds: the N of Go's *[N]T, supplied by the minting site. It
    // bounds every element take through this box (see the class remarks).
    private readonly nint m_length;

    private NativeArrayBox(nuint nativeAddress, nint length)
        : base(isNull: nativeAddress == 0)
    {
        m_nativeAddr = nativeAddress;
        m_length = length;

        // The box only. The block it names is native — never charged, because the CLR heap never
        // received it — matching NativeBox's leaf-ctor counting.
        AllocationCounter.Count();
    }

    /// <summary>
    /// The single creation door: a pointer to <paramref name="length"/> contiguous <typeparamref name="T"/>
    /// at <paramref name="nativeAddress"/>. A zero address is the nil pointer, as everywhere else
    /// (<c>(*[N]T)(unsafe.Pointer(uintptr(0))) == nil</c>); a negative length is a caller defect and
    /// is refused by name rather than clamped.
    /// </summary>
    internal static ж<array<T>> Over(nuint nativeAddress, nint length)
    {
        if (length < 0)
            throw new PanicException($"native-backed array: negative length {length}");

        return nativeAddress == 0 ? ж<array<T>>.NilBox : new NativeArrayBox<T>(nativeAddress, length);
    }

    /// <summary>
    /// The element view, and the only honest way through this box. A <c>slice&lt;T&gt;</c> over the
    /// same block is already an <see cref="IArray{T}"/> (<c>ISlice&lt;T&gt; : IArray&lt;T&gt;</c>),
    /// so the element door needs no second native array implementation — it reuses the one creation
    /// door golib already has for native-backed windows, refusals included (a managed-reference
    /// element type is refused there, by name).
    /// </summary>
    internal override IArray<Telem>? TryGetNativeArrayView<Telem>()
    {
        // The block holds T. A request for any other element type is not this box's to answer, and
        // falling through is right: the caller then meets the ordinary not-an-array-or-slice error
        // rather than a window over bytes that mean something else.
        if (typeof(Telem) != typeof(T))
            return null;

        if (IsNilPointer)
            return null;

        return (IArray<Telem>)(object)slice<T>.OverNativeMemory(m_nativeAddr, m_length);
    }

    /// <inheritdoc/>
    // There is no array<T> at this address to return a ref to — see the class remarks. Refusing by
    // name is the whole point of the kind: the alternative is the garbage reference that made the
    // page allocator die in a prestub with an empty stderr.
    public override ref array<T> Value => throw noMaterializableArray();

    /// <inheritdoc/>
    public override ref array<T> ValueSlot => throw noMaterializableArray();

    private PanicException noMaterializableArray() =>
        new($"native-backed array at 0x{m_nativeAddr:x}: a *[N]{typeof(T).Name} over native memory " +
            "has no array<T> to dereference (Go's *[N]T points at bare elements, with no header); " +
            "reach the elements through the element door instead");

    /// <inheritdoc/>
    public override nuint NativeAddress => m_nativeAddr;

    /// <summary>The element count this box was minted with.</summary>
    internal nint Length => m_length;

    /// <inheritdoc/>
    // Two boxes over one address are the same Go pointer, as for every native-backed kind.
    public override nuint PointerOrderToken => IsNilPointer ? 0 : m_nativeAddr;

    /// <inheritdoc/>
    public override bool Equals(ж<array<T>>? other)
    {
        if (other is null)
            return IsNilPointer;

        if (ReferenceEquals(this, other))
            return true;

        if (other is NativeArrayBox<T> nab)
            return m_nativeAddr == nab.m_nativeAddr;

        return IsNilPointer && other.IsNilPointer;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => IsNilPointer ? 0 : m_nativeAddr.GetHashCode();
}
