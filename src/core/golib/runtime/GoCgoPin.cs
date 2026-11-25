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

    // TryIdentity for a pointee known only at run time (the word mirror's element type, a ж<U>).
    internal bool TryIdentityOf(nuint address, Type elementType, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out object? box)
    {
        box = address == m_start && m_box is not null && elementType.IsInstanceOfType(m_box) ? m_box : null;
        return box is not null;
    }
}

/// <summary>
/// The whole backing array of a Go array (or slice) of C pointers, as words C can read and fill (cgo Phase 3, .5 cgo
/// item 2, the argv shape: DESIGN-cgo-phase3-marshaling 18.12.3, H's verdict (A)). <c>C.f(&amp;argv[0])</c> hands C a
/// pointer to the first of the array's C pointers, and Go lets C read or write the whole array. go2cs's Go array holds
/// BOXES, not words, so the door copies every element into a word array of the same length (the copy-in is a pointer
/// member's: nil is 0, a C pointer its address, a Go pointer cgo's own panic, B-10), the wrapper pins the word array for
/// the call and hands C the address of the element's word, and after the call each word is copied back: a word C left
/// alone keeps its element, 0 is nil, the address of a stack copy the wrapper made is that copy's box, and any other
/// address is C memory.
/// </summary>
/// <remarks>
/// The cost is the backing array's length, not the element count C uses: every element is copied in and out on every
/// call, and a re-slice (<c>argv[2:]</c>) keeps its whole backing array, so a short slice of a long array pays for the
/// long one. That is the price of letting C touch every word Go would let it touch; docs/Limitations.md#cgo says so.
/// A word C stores that points into Go memory the call pinned (the word array itself included: <c>argv[0] = &amp;argv[2]</c>)
/// is refused by name inside the wrapper's <c>fixed</c>, as any word inside a pin is: Go would keep it, but go2cs holds
/// the array as boxes, so a pointer into its words has nothing to stand for once the call returns.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoCgoWordMirror
{
    private readonly object?[] m_storage;
    private readonly Type m_elementType;
    private readonly nint[] m_words;
    private readonly nint[] m_copiedIn;
    private readonly Func<nuint, object>? m_mint;

    // storage is the element storage (a ж<U>[], seen through array covariance), mint makes the box for a C address C
    // wrote (null when the door cannot name the pointee type: the void door, which mints through an element instead).
    internal GoCgoWordMirror(object?[] storage, Func<nuint, object>? mint)
    {
        m_storage = storage;
        m_elementType = storage.GetType().GetElementType()!;
        m_mint = mint;
        m_words = new nint[storage.Length];
        m_copiedIn = new nint[storage.Length];

        for (int i = 0; i < storage.Length; i++)
            m_copiedIn[i] = m_words[i] = storage[i] is ICgoWord element ? (nint)element.CgoWord() : 0;
    }

    // The pin for the word that stands for element `index`: the word array, at that word.
    internal GoCgoPin PinAt(nint index) => new(m_words, index * IntPtr.Size);

    /// <summary>
    /// Called inside the wrapper's <c>fixed</c>, after the call, against each pinned argument (this mirror's own pin
    /// included): a word C wrote that lies in that argument's Go storage is refused by name.
    /// </summary>
    public unsafe void RequireWordsOutsidePin(in GoCgoPin pin, byte* pinned, string function)
    {
        for (int i = 0; i < m_words.Length; i++)
        {
            if (m_words[i] != m_copiedIn[i])
                GoCgoArgs.RequireWordOutsidePin((nuint)m_words[i], pin, pinned, function);
        }
    }

    /// <summary>
    /// Copies every word back into its element after the call: a word C left alone keeps its element, 0 is nil, a
    /// stack copy's own address is the box that copy stands for, an address elsewhere inside a stack copy is refused by
    /// name, and any other address is C memory.
    /// </summary>
    public void CopyBack(ReadOnlySpan<GoCgoStackCopy> copies, string function)
    {
        for (int i = 0; i < m_words.Length; i++)
        {
            nuint word = (nuint)m_words[i];

            if (m_words[i] == m_copiedIn[i])
                continue;

            m_storage[i] = word == 0 ? null : ElementFor(word, copies, function);
        }
    }

    private object ElementFor(nuint word, ReadOnlySpan<GoCgoStackCopy> copies, string function)
    {
        foreach (ref readonly GoCgoStackCopy copy in copies)
        {
            if (copy.TryIdentityOf(word, m_elementType, out object? box))
                return box;
        }

        foreach (ref readonly GoCgoStackCopy copy in copies)
        {
            if (copy.Holds(word))
                throw RuntimeErrorPanic.ErrorString($"cgo: C.{function} returned an address inside the stack copy of a Go argument that is not that argument as an element of {m_elementType.Name}, and go2cs cannot represent a pointer into that copy, so it refuses the result");
        }

        if (m_mint is not null)
            return m_mint(word);

        foreach (object? element in m_storage)
        {
            if (element is ICgoWord sibling)
                return sibling.CgoNativeSibling(word);
        }

        throw RuntimeErrorPanic.ErrorString($"cgo: C.{function} wrote a C pointer into a Go array of C pointers handed to it as a void pointer, and with every element nil go2cs cannot name the pointee type to hold it, so it refuses the result");
    }
}

// A pointer box as one word of a Go array of C pointers (ж.CgoPin.cs), read without the pointee type: the word the
// copy-in hands C (a pointer member's rule), and a native box of the same pointee type for a C address C wrote.
internal interface ICgoWord
{
    nuint CgoWord();

    object CgoNativeSibling(nuint address);
}

// An element of a Go array (ElemRefBox), asked by the void door whether it is an element of managed storage of
// pointer boxes, and so crosses as a word mirror of the whole backing array.
internal interface ICgoWordElement
{
    bool TryCgoWordMirror([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out GoCgoWordMirror? mirror, out nint index);
}

// The pin a box owns, read without the pointee type (the cgo door sees the box as an object). Implemented by ж<T>
// (ж.CgoPin.cs): the address-take's pin-only hold, or the pin a native box took over managed storage.
internal interface IManagedPinHolder
{
    PinnedBuffer? ManagedPin { get; }
}
