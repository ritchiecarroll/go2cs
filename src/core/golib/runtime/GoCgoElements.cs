// GoCgoElements.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace go.golib;

/// <summary>
/// How a cgo wrapper checks one pointer member of a struct it copies into a mirror, as cmd/cgo's check would for the
/// call's syntax (docs/phase4/DESIGN-cgo-struct-array-element.md section 2).
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public enum GoCgoCheck
{
    /// <summary>
    /// The address and element forms (<c>C.f(&amp;x)</c>, <c>C.f(&amp;a[i])</c> inside its slice): cmd/cgo checks the
    /// pointee by its type, and the runtime's panic names the member's kind ("pointer" for a typed member, "unsafe
    /// pointer" for a <c>void *</c> member: cgocall.go, cgoFormatErr).
    /// </summary>
    Kind,

    /// <summary>
    /// The variable form (<c>p := &amp;x; C.f(p)</c>): the runtime checks the whole heap object (cgoCheckUnknownPointer)
    /// and its panic says "pointer" whatever the member's kind.
    /// </summary>
    Whole,

    /// <summary>
    /// An element of the backing array BEFORE the element form's slice: Go checks nothing there, and C may still read it.
    /// A Go pointer in its pointer member crosses as 0, since a mirror cannot carry a Go address.
    /// </summary>
    Outside
}

/// <summary>
/// The check window of one struct-pointer argument, as the converter's fold of cmd/cgo's check closure knows it
/// (DESIGN-cgo-struct-array-element.md section 4): the default is the address form, <see cref="Variable"/> the variable
/// form, and <see cref="Of{T}(slice{T})"/> the element form, which carries the slice cmd/cgo checks (gcc.go, checkIndex:
/// from its first element through its capacity). It travels BESIDE the pointer, so the element box stays the program's
/// own <c>&amp;a[i]</c>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public readonly struct GoCgoWindow
{
    private enum Form : byte { Address, Variable, Slice }

    private readonly Form m_form;
    private readonly object? m_backing;
    private readonly nint m_start;
    private readonly nint m_end;

    private GoCgoWindow(Form form, object? backing, nint start, nint end)
    {
        m_form = form;
        m_backing = backing;
        m_start = start;
        m_end = end;
    }

    /// <summary>The variable form: the whole heap object is checked, and the panic says "pointer".</summary>
    public static GoCgoWindow Variable => new(Form.Variable, null, 0, 0);

    /// <summary>The element form over a slice: its elements from its start through its capacity are checked.</summary>
    public static GoCgoWindow Of<T>(slice<T> s) =>
        s.IsNativeBacked || s.m_array is null ? default : new(Form.Slice, s.m_array, s.Low, s.Low + s.Capacity);

    /// <summary>The element form over an array: its elements are checked.</summary>
    public static GoCgoWindow Of<T>(array<T> a) =>
        a.m_array is null ? default : new(Form.Slice, a.m_array, a.Low, a.Low + a.Length);

    /// <summary>The element form over a pointer to an array (<c>(*p)[i]</c>).</summary>
    public static GoCgoWindow Of<T>(ж<array<T>>? p) => p is null || p.IsNilPointer ? default : Of(p.Value);

    /// <summary>The check of a single struct, handed by itself (the mirror door).</summary>
    public GoCgoCheck Single => m_form == Form.Variable ? GoCgoCheck.Whole : GoCgoCheck.Kind;

    /// <summary>The check of the element at <paramref name="index"/> of <paramref name="backing"/>.</summary>
    public GoCgoCheck At(object backing, nint index)
    {
        switch (m_form)
        {
            case Form.Variable:
                return GoCgoCheck.Whole;

            case Form.Slice when ReferenceEquals(m_backing, backing):
                // Go checks the slice from its start through its capacity; an element before it is C's to read and
                // nothing Go checks. One past the capacity cannot be in the backing array's own span here.
                return index >= m_start && index < m_end ? GoCgoCheck.Kind : GoCgoCheck.Outside;

            default:
                return GoCgoCheck.Kind;
        }
    }
}

/// <summary>A struct mirror's copy-in under a check (the wrapper's <c>ᴍin_M</c>).</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate void GoCgoCopyIn<M, V>(ref M m, in V v, GoCgoCheck check);

/// <summary>
/// A struct mirror's copy-out against the snapshot of what was copied in (the wrapper's <c>ᴍout_M</c> with a snapshot):
/// a pointer word equal to the snapshot's keeps the original member, and no member function runs on the original.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate void GoCgoCopyOutSnapshot<V, M>(ref V v, in M m, in M s, ReadOnlySpan<GoCgoStackCopy> copies, string function);

/// <summary>A struct mirror's pointer members checked against one pinned argument (the wrapper's <c>ᴍpin_M</c>).</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public unsafe delegate void GoCgoPinCheck<M>(in M m, in GoCgoPin pin, byte* pinned, string function);

/// <summary>
/// Door 1 (docs/phase4/DESIGN-cgo-struct-array-element.md): an element of a Go array or slice of C structs handed to C.
/// cgo lets C touch the whole backing array, so the whole backing array is mirrored, from index 0, into an array of the
/// struct's mirror that stays pinned for its lifetime; the element's mirror is what C receives. After the call each
/// element whose mirror bytes C changed is copied back against the snapshot of what went in, so a goroutine's write to
/// an element C did not touch survives, and a pointer word C left alone keeps its original member.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public sealed unsafe class GoCgoElements<V, M> where M : unmanaged
{
    private readonly V[] m_backing;
    private readonly M[] m_mirrors;
    private readonly M[] m_snapshot;
    private readonly nint m_index;

    /// <summary>Mirrors <paramref name="backing"/> whole, each element under the window's check.</summary>
    public GoCgoElements(V[] backing, nint index, GoCgoWindow window, GoCgoCopyIn<M, V> copyIn)
    {
        m_backing = backing;
        m_index = index;

        // Pinned for its lifetime, so the pin checks inside the wrapper's `fixed` and the result mapping after it read
        // one address.
        m_mirrors = GC.AllocateArray<M>(backing.Length, pinned: true);

        for (int i = 0; i < backing.Length; i++)
            copyIn(ref m_mirrors[i], backing[i], window.At(backing, i));

        m_snapshot = (M[])m_mirrors.Clone();
    }

    private nuint Base => (nuint)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(m_mirrors));

    private nuint Size => (nuint)(m_mirrors.Length * sizeof(M));

    /// <summary>The pin the wrapper's <c>fixed</c> holds: the mirror array, at the element C receives.</summary>
    public GoCgoPin Pin => new(m_mirrors, m_index * sizeof(M));

    /// <summary>Every element's pointer members, checked against one pinned argument (this one's included).</summary>
    public void RequirePointersOutsidePin(GoCgoPinCheck<M> check, in GoCgoPin pin, byte* pinned, string function)
    {
        for (int i = 0; i < m_mirrors.Length; i++)
            check(in m_mirrors[i], pin, pinned, function);
    }

    /// <summary>Whether an address lies in the mirror array (or one past its end).</summary>
    public bool Contains(nuint address) => address >= Base && address <= Base + Size;

    /// <summary>
    /// Copies back each element whose mirror bytes C changed, against the snapshot of what went in.
    /// </summary>
    public void CopyBack(GoCgoCopyOutSnapshot<V, M> copyOut, ReadOnlySpan<GoCgoStackCopy> copies, string function)
    {
        for (int i = 0; i < m_mirrors.Length; i++)
        {
            ReadOnlySpan<byte> now = MemoryMarshal.AsBytes(new ReadOnlySpan<M>(in m_mirrors[i]));
            ReadOnlySpan<byte> before = MemoryMarshal.AsBytes(new ReadOnlySpan<M>(in m_snapshot[i]));

            if (!now.SequenceEqual(before))
                copyOut(ref m_backing[i], m_mirrors[i], m_snapshot[i], copies, function);
        }
    }

    /// <summary>
    /// A pointer result inside the mirror array: on element k's boundary it is that element, the program's own
    /// <c>&amp;a[k]</c>, a plain box; an interior word or the address one past the end is refused by name.
    /// </summary>
    public ж<V> ElementAt(nuint address, string function)
    {
        nuint offset = address - Base;

        if (address == Base + Size)
            throw RuntimeErrorPanic.ErrorString($"cgo: C.{function} returned the address one past the end of the Go array it was handed an element of, and go2cs cannot represent a pointer past a Go array's end, so it refuses the result");

        if (offset % (nuint)sizeof(M) != 0)
            throw RuntimeErrorPanic.ErrorString($"cgo: C.{function} returned an address inside an element of the Go array it was handed an element of, not an element's own address, and go2cs cannot represent a pointer into the middle of an element, so it refuses the result");

        return ElemRefBox<V>.OfArrayElement(new array<V>(m_backing), (int)(offset / (nuint)sizeof(M)));
    }
}
