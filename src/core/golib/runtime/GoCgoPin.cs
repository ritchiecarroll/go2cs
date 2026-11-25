// GoCgoPin.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace go.golib;

/// <summary>
/// One cgo argument as the wrapper hands it to C (cgo Phase 3, increment 3.2, seat 1b:
/// docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.12): a number (nil or C memory), or an address inside Go storage that
/// the wrapper holds still for the call. The wrapper pins it with a pattern-based <c>fixed</c>
/// (<c>fixed (byte* f = pin)</c>), whose pinnable reference is the storage's first byte, or a null reference for a
/// number, which pins nothing; <see cref="Address"/> then answers what C receives.
/// </summary>
/// <remarks>
/// The pin's extent is the <c>fixed</c> block, so it needs no handle, no finalizer and no index entry, and nothing leaks
/// when a later argument's classification throws (design 13.5: classify every argument, then pin, then call). A cgo
/// wrapper is therefore synchronous: never async, never an iterator, never rewritten into a resumable twin.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public readonly struct GoCgoPin
{
    private readonly Array? m_root;
    private readonly nint m_offset;
    private readonly nuint m_number;

    // A number: nil (0) or C memory.
    internal GoCgoPin(nuint number)
    {
        m_number = number;
    }

    // An address inside Go storage: the storage, and the byte offset inside it.
    internal GoCgoPin(Array root, nint offset)
    {
        m_root = root;
        m_offset = offset;
    }

    /// <summary>Whether this argument is Go memory the wrapper must hold still for the call.</summary>
    public bool IsGoMemory => m_root is not null;

    /// <summary>The reference a <c>fixed</c> pins: the storage's first byte, or a null reference for a number.</summary>
    public ref byte GetPinnableReference() => ref m_root is null ? ref Unsafe.NullRef<byte>() : ref MemoryMarshal.GetArrayDataReference(m_root);

    /// <summary>What C receives, given the address the wrapper's <c>fixed</c> pinned this argument at.</summary>
    public unsafe nint Address(byte* pinned) => m_root is null ? (nint)m_number : (nint)(pinned + m_offset);

    // Whether an address C returned lies in this argument's storage, asked while the wrapper's `fixed` still pins it at
    // `pinned`: [first byte, end of the storage]. One past the end is included: C's write cursor `buf + n` is an address
    // in the managed heap, not C memory, and it goes stale once the `fixed` ends (P3's read of the fold, item 1).
    internal unsafe bool Holds(byte* pinned, nuint address)
    {
        if (m_root is null || pinned == null)
            return false;

        nuint first = (nuint)pinned;
        nuint end = (nuint)(nint)Marshal.UnsafeAddrOfPinnedArrayElement(m_root, m_root.Length);

        return address >= first && address <= end;
    }
}

/// <summary>
/// One stack copy a cgo wrapper handed C the address of (a mirror of a whole Go struct or one Go scalar, or an
/// out-parameter's word), with the Go box it stands for (cgo Phase 3, .5 cgo items 1(a) and 1(b): the C-return routes,
/// DESIGN-cgo-phase3-marshaling 18.12.3). A word C returns (a pointer result, an out-parameter's word, a pointer member
/// of a mirror) that equals the copy's own address IS that box, as it is under Go (<c>pick(&amp;a, &amp;out)</c> leaves
/// <c>out == &amp;a</c>); a word anywhere else inside the copy names a stack slot that dies with the call, and is
/// refused by name. A default value (no address) stands for an argument that was not copied: it matches nothing.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public readonly struct GoCgoStackCopy
{
    private readonly nuint m_start;
    private readonly nuint m_size;
    private readonly object? m_box;

    /// <summary>
    /// A stack copy of <paramref name="size"/> bytes at <paramref name="start"/>, standing for <paramref name="box"/>.
    /// </summary>
    public unsafe GoCgoStackCopy(void* start, nuint size, object? box)
    {
        m_start = (nuint)start;
        m_size = size;
        m_box = box;
    }

    // Whether the copy was made (an argument mirrored for this call).
    internal bool IsCopy => m_start != 0;

    // The box this copy stands for, when the address is the copy's own and the box is a pointer of C's pointee type.
    internal bool TryIdentity<U>(nuint address, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ж<U>? box)
    {
        box = address == m_start ? m_box as ж<U> : null;
        return box is not null;
    }

    // Whether an address lies in the copy: [its first byte, one past its end]. One past the end is included for the
    // pin's reason (GoCgoPin.Holds): C's cursor past a copy is a stack address, not C memory.
    internal bool Holds(nuint address) => m_start != 0 && address >= m_start && address <= m_start + m_size;
}

// The pin a box owns, read without the pointee type (the cgo door sees the box as an object). Implemented by ж<T>
// (ж.CgoPin.cs): the address-take's pin-only hold, or the pin a native box took over managed storage.
internal interface IManagedPinHolder
{
    PinnedBuffer? ManagedPin { get; }
}
