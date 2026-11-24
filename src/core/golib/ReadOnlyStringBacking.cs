// ReadOnlyStringBacking.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using go.golib;

namespace go;

/// <summary>
/// The private copy that <c>unsafe.StringData</c> hands out for a string whose bytes are STATIC data: a string
/// literal (<see cref="StringLiteralCache"/>) or a one-byte string from <see cref="@string"/>'s table.
/// </summary>
/// <remarks>
/// <para>
/// WHY A COPY. Go keeps a literal's bytes, and the one-byte strings of <c>runtime.staticuint64s</c>, in read-only
/// memory, so a write through <c>unsafe.StringData</c> FAULTS (SIGSEGV) and nothing else ever sees the new byte. Here
/// those bytes are ordinary managed arrays shared by the whole process: one backing per literal and one per byte
/// value. A write through a pointer into them changed every later equal literal and every later equal one-byte
/// string, in every package. A HEAP string's bytes are not static in Go either: a write through its data pointer
/// mutates every string sharing them, and go2cs does the same, so a heap backing is never registered here.
/// </para>
/// <para>
/// ONE SHADOW PER BACKING, made on the first <c>StringData</c> over it, never a copy per call. Every pointer into a
/// static backing then names the same managed storage, so pointer identity holds as Go's does:
/// <c>StringData(lit) == StringData(lit)</c>, and <c>unsafe.Add(StringData(lit), 1) == StringData(lit[1:])</c>. The
/// <c>Data</c> of a <c>reflect.StringHeader</c> laid over such a string (<c>StringHeaderBox</c>) is minted through
/// <see cref="TryDataBox"/> too, so it equals <c>StringData</c> as in Go, and the element box answers the shadow as its
/// run (<c>IElementRun</c>), so a header store of that <c>Data</c> rebuilds the string over the shadow.
/// </para>
/// <para>
/// THE NEXT-ACCESS CHECK. A box cannot observe a write performed through the ref it has already handed out, so the
/// write itself is not caught. What is caught is the NEXT access that goes through a hook: a dereference of an
/// element box over the shadow (<see cref="this[nint]"/>, which is <see cref="ElemRefBox{T}.Value"/> on this box's
/// foreign arm, and every box <c>unsafe.Add</c> derives from it), and every later <c>StringData</c> over the same
/// backing (<see cref="ThrowIfWritten"/>, which compares the whole backing). Either one panics by name rather than
/// letting the program read a value Go would never have let it write.
/// </para>
/// <para>
/// THE LIMIT. A view that reads the storage directly takes no hook: a slice from <c>unsafe.Slice</c>, and an array
/// pointer view (<c>(*[N]byte)(unsafe.Pointer(p))</c>, <see cref="ElemRefBox{T}.TryGetElementStorage"/>). A write
/// through one lands in the shadow, so it still reaches nothing else, but it is reported only if a later access goes
/// through a hook. Go faults at the write itself; no managed hook can.
/// </para>
/// <para>
/// Guarded by GolibTests ReadOnlyStringDataTests.
/// </para>
/// </remarks>
internal sealed class ReadOnlyStringBacking : IArray<byte>
{
    // Weak on the origin: a literal's origin lives as long as the literal cache holds it (the process), and a
    // one-byte backing as long as the table (the process). A shadow never outlives its origin's entry.
    private static readonly ConditionalWeakTable<byte[], ReadOnlyStringBacking> s_registered = new();

    private readonly byte[] m_origin;
    private byte[]? m_shadow;

    private ReadOnlyStringBacking(byte[] origin)
    {
        m_origin = origin;
    }

    /// <summary>
    /// Records <paramref name="origin"/> as static data: the backing of a cached string literal.
    /// </summary>
    internal static void Register(byte[] origin)
    {
        if (origin.Length != 0)
            s_registered.TryAdd(origin, new ReadOnlyStringBacking(origin));
    }

    /// <summary>
    /// The read-only record for <paramref name="backing"/>, or null when it is not static data.
    /// </summary>
    internal static ReadOnlyStringBacking? Of(byte[] backing)
    {
        if (s_registered.TryGetValue(backing, out ReadOnlyStringBacking? found))
            return found;

        // The 256 one-byte backings are known by reference, so they are recorded on first use rather than at start.
        if (@string.IsOneByteTableBacking(backing))
            return s_registered.GetValue(backing, static origin => new ReadOnlyStringBacking(origin));

        return null;
    }

    /// <summary>
    /// <c>unsafe.StringData</c>'s pointer for a non-empty string window: an element box over the shadow when the
    /// window's backing is static data, otherwise null.
    /// </summary>
    internal static ж<byte>? TryDataBox(in slice<byte> window)
    {
        if (Of(window.m_array) is not { } readOnly)
            return null;

        readOnly.ThrowIfWritten();

        // The foreign arm: ElemRefBox's canonical pair resolves this wrapper to (Source, absolute index), the shadow,
        // so identity, pinning and views treat the box as an element of the shadow, while Value comes through the
        // checked indexer below.
        return new ElemRefBox<byte>((IArray)readOnly, (int)window.Low);
    }

    // Made once, on the first StringData over the backing. A plain allocation, not charged to AllocationCounter: like
    // the one-byte table it stands for static data, which no conversion allocates.
    private byte[] Shadow => Volatile.Read(ref m_shadow) ?? Interlocked.CompareExchange(ref m_shadow, (byte[])m_origin.Clone(), null) ?? m_shadow!;

    /// <summary>
    /// Panics by name when any byte of the shadow differs from the static data it copies.
    /// </summary>
    internal void ThrowIfWritten()
    {
        byte[]? shadow = Volatile.Read(ref m_shadow);

        if (shadow is not null && !shadow.AsSpan().SequenceEqual(m_origin))
            throw RuntimeErrorPanic.WriteThroughReadOnlyStringData();
    }

    /// <summary>
    /// The element at <paramref name="index"/> of the shadow, after checking that it still holds the static byte.
    /// </summary>
    public ref byte this[nint index]
    {
        get
        {
            byte[] shadow = Shadow;

            if (shadow[index] != m_origin[index])
                throw RuntimeErrorPanic.WriteThroughReadOnlyStringData();

            return ref shadow[index];
        }
    }

    object? IArray.this[nint index]
    {
        get => this[index];
        set => this[index] = (byte)value!;
    }

    // The canonical storage: ElemRefBox's canonical resolution reads IArray.Source for a container it does not know.
    public byte[] Source => Shadow;

    Array? IArray.Source => Shadow;

    public nint Length => m_origin.Length;

    public Span<byte> ꓸꓸꓸ => Shadow;

    public Span<byte> ToSpan() => Shadow;

    public object Clone() => (byte[])Shadow.Clone();

    public IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)Shadow).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
