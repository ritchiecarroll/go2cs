//******************************************************************************************************
//  ж.ShadowArrayBox.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/28/2026 - R (go2cs fleet)
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// A Go <c>*[N]T</c> over a NATIVE block whose element type <typeparamref name="T"/> carries managed
/// references: the elements live in a MANAGED <see cref="array{T}"/> that shadows the block, while the
/// pointer's ADDRESS stays the block's.
/// </summary>
/// <remarks>
/// <para>
/// WHY A SHADOW (A16, COORD ruling 2026-09-28). The page allocator's grow installs each L2 chunk block
/// as <c>*(*uintptr)(unsafe.Pointer(&amp;p.chunks[l1])) = uintptr(sysAlloc(l2Size))</c>, a pointer to
/// <c>[8192]pallocData</c> over native memory. <see cref="NativeArrayBox{T}"/> serves that shape only for
/// an UNMANAGED element: <c>pallocData</c> is a generated struct over golib <see cref="array{T}"/>s, a
/// managed type at two levels, so no element ref can alias native bytes and the native door refuses it
/// by name. Neither a native view (the bytes are not C#'s layout) nor a pinned array (the CLR pins no
/// reference-bearing array) can hold it; a managed store can.
/// </para>
/// <para>
/// WHY THE ADDRESS STAYS NATIVE. Go keeps using the block as memory: export_test's
/// <c>FreePageAlloc</c> hands each block to <c>sysFree</c>, and <c>enableChunkHugePages</c> to
/// <c>sysHugePage</c>. A plain managed box answers <c>ж -&gt; uintptr</c> by pinning its array's data,
/// which the CLR refuses for a reference-bearing array ("Object contains references", measured on the
/// windows re-probe) -- and even an order token would hand the kernel an address it never allocated.
/// Answering the block's own address as <see cref="NativeAddress"/> makes every address consumer see
/// exactly what Go's does, and the memory Go allocated and accounts for (sysStat) is released by Go's
/// own call. The block itself is never read or written: it stands unused behind the managed store, as
/// Go's memory does behind the elements.
/// </para>
/// <para>
/// WHAT READS WHAT. Element access (<c>chunkOf</c>'s <c>&amp;p.chunks[l1][l2]</c>) goes through
/// <see cref="Value"/>, the managed array, whose element boxes are ordinary managed ones.
/// <see cref="TryGetNativeArrayView{Telem}"/> answers null, so the element door never builds a native
/// window over the block. The address (<see cref="NativeAddress"/>, <see cref="PointerOrderToken"/>)
/// is the block's. <c>NativeAddress</c>-gated byte arithmetic (<c>unsafe.Add</c>'s byte stepping,
/// <c>ReadPointerWord</c>) would therefore address the UNUSED block; no corpus site does arithmetic on
/// this pointer (Go indexes it), and a new one would read zeros rather than the store -- stated here so
/// it is found, not assumed away.
/// </para>
/// </remarks>
internal sealed class ShadowArrayBox<T> : ж<array<T>>
{
    private readonly nuint m_nativeAddr;
    private array<T> m_store;

    private ShadowArrayBox(nuint nativeAddress, nint length)
        : base(isNull: false)
    {
        m_nativeAddr = nativeAddress;

        // Zeroed, as sysAlloc'd memory is: Go reads a fresh L2 block as all-zero pallocData.
        m_store = new array<T>(length);

        AllocationCounter.Count();
    }

    /// <summary>
    /// The creation door: a pointer to <paramref name="length"/> reference-bearing <typeparamref name="T"/>
    /// "at" <paramref name="nativeAddress"/>. A zero address is the nil pointer, as for every native kind.
    /// </summary>
    internal static ж<array<T>> Over(nuint nativeAddress, nint length)
    {
        if (length < 0)
            throw new PanicException($"native-shadowed array: negative length {length}");

        return nativeAddress == 0 ? ж<array<T>>.NilBox : new ShadowArrayBox<T>(nativeAddress, length);
    }

    /// <inheritdoc/>
    public override ref array<T> Value => ref m_store;

    /// <inheritdoc/>
    public override ref array<T> ValueSlot => ref m_store;

    /// <inheritdoc/>
    // The block's address: the one Go allocated, frees and advises (see the class remarks).
    public override nuint NativeAddress => m_nativeAddr;

    /// <inheritdoc/>
    // The elements are managed, so no native window is ever built over the block.
    internal override IArray<Telem>? TryGetNativeArrayView<Telem>() => null;

    /// <inheritdoc/>
    // No MANAGED storage to pin or name by address: the address is native, answered first by the
    // conversion operators (NativeAddress), exactly as for NativeArrayBox.
    public override PointerStorage StorageKind => PointerStorage.None;

    /// <inheritdoc/>
    // Two pointers to one block are the same Go pointer.
    public override nuint PointerOrderToken => m_nativeAddr;

    /// <inheritdoc/>
    public override bool Equals(ж<array<T>>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return other is ShadowArrayBox<T> shadow && m_nativeAddr == shadow.m_nativeAddr;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => m_nativeAddr.GetHashCode();

    /// <summary>Whether a native array pointer over <typeparamref name="T"/> needs this kind (a reference-bearing element).</summary>
    internal static bool Needed => RuntimeHelpers.IsReferenceOrContainsReferences<T>();
}
