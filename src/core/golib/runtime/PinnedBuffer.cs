// PinnedBuffer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace go.golib;

/// <summary>
/// Represents a pinned buffer that can be used as an array pointer reference.
/// </summary>
/// <remarks>
/// Buffer is allocated with a pinned handle so it can be used in unmanaged code.
/// Because buffer is pinned, it will not be moved by the garbage collector.
/// Pinned buffer class is disposable and registered with the garbage collector
/// so allocated handle will be freed when class is no longer referenced.
/// </remarks>
internal class PinnedBuffer : IArray<byte>, IDisposable
{
    private GCHandle m_handle;
    private readonly int m_len;
    private bool m_disposed;

    // Set, under the pin-extent index's backfill gate, as a buffer made BEFORE the index switched on is released, so a
    // backfill racing the release cannot enter it afterwards. A bool, so it sits in the object's padding: a process that
    // links no cgo package allocates the same 32-byte buffer it did before the index existed (R6 holds that number).
    private bool m_closed;

    // Not constructible directly: Of picks the indexed kind when a cgo package has switched the index on, so a buffer
    // made any other way could never enter the index, and a missed pin is the door's unsafe direction.
    private protected PinnedBuffer(byte[] array)
    {
        m_handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        m_len = array.Length;
    }

    private protected PinnedBuffer(object? src, int len)
    {
        m_handle = src is null ? default : GCHandle.Alloc(src, GCHandleType.Pinned);
        m_len = len;
    }

    /// <summary>
    /// Pins <paramref name="array"/>. In a process where a cgo package has switched the pin-extent index on, the buffer
    /// is the indexed kind, which carries its entry; anywhere else it is this one, and the index costs it nothing.
    /// </summary>
    internal static PinnedBuffer Of(byte[] array) =>
        PinExtents.Current is null ? new PinnedBuffer(array) : new IndexedPinnedBuffer(array);

    /// <summary>Pins <paramref name="src"/>, as <see cref="Of(byte[])"/> does.</summary>
    internal static PinnedBuffer Of(object? src, int len) =>
        PinExtents.Current is null ? new PinnedBuffer(src, len) : new IndexedPinnedBuffer(src, len);

    /// <summary>Pins <paramref name="src"/>, as <see cref="Of(byte[])"/> does.</summary>
    internal static PinnedBuffer Of(object? src, nint len) => Of(src, (int)len);

    ~PinnedBuffer()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (m_disposed)
            return;

        // Out of the index BEFORE the pin is released: from here on the extent's bytes may move.
        LeaveIndex();

        if (m_handle.IsAllocated)
            m_handle.Free();

        m_disposed = true;
    }

    /// <summary>
    /// Pins <paramref name="storage"/> for the returned buffer's lifetime and nothing more, or
    /// returns <c>null</c> when the runtime refuses to pin it.
    /// </summary>
    /// <param name="storage">Managed object to hold still.</param>
    /// <remarks>
    /// <para>
    /// The result is a PIN OWNER rather than a view: its length is zero, because the caller's
    /// interest is that the object's address stop moving, not the bytes behind it. Everything a
    /// buffer offers over its contents — the indexer, the span, the enumerator — is therefore empty
    /// here, and only <see cref="Pointer"/> and the pin's own lifetime are meaningful.
    /// </para>
    /// <para>
    /// <c>GCHandle</c> pins only blittable objects and throws <see cref="ArgumentException"/> for
    /// anything else, which for a caller asking "can this be held still?" is an answer rather than a
    /// failure — hence the <c>null</c> instead of the throw. A caller that cannot get a pin keeps
    /// whatever it did before pinning was available.
    /// </para>
    /// </remarks>
    internal static PinnedBuffer? PinOnly(object storage)
    {
        try
        {
            return Of(storage, 0);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public unsafe byte* Pointer => (byte*)m_handle.AddrOfPinnedObject();

    /// <summary>
    /// The index's backfill, for a pin taken before a cgo package switched the index on: enters this buffer's extent,
    /// keyed by its handle, unless it is already in or already released. Idempotent. The indexed kind enters at
    /// construction instead, and its override does nothing.
    /// </summary>
    internal virtual void EnterIndex() => PinExtents.Current?.EnterBackfilled(this);

    // Out of the index, before the pin is released. Here only a backfilled buffer can be in it, and a process with no
    // index pays one read of it.
    private protected virtual void LeaveIndex() => PinExtents.Current?.LeaveBackfilled(this);

    // Under the backfill gate only.
    internal bool Closed
    {
        get => m_closed;
        set => m_closed = value;
    }

    internal nint HandleKey => GCHandle.ToIntPtr(m_handle);

    /// <summary>
    /// The extent [start, end) of the pinned OBJECT's data, and not this buffer's own length (a pin-only hold is length 0),
    /// or false when the buffer pins nothing.
    /// </summary>
    /// <remarks>
    /// It reads the handle with no fence against a concurrent explicit Dispose (P3's review, item 4), and nothing can
    /// race one. The indexed kind calls it only from its own constructor, before anything else can see the buffer. The
    /// backfill calls it under the backfill gate, which a release must take (LeaveBackfilled) before it frees the handle.
    /// </remarks>
    internal bool TryExtent(out nuint start, out nuint end)
    {
        start = end = 0;

        if (!m_handle.IsAllocated || m_handle.Target is not { } target)
            return false;

        start = (nuint)m_handle.AddrOfPinnedObject();
        nuint length = PinExtents.ExtentOf(target, m_len);
        end = start + length;
        return start != 0 && length != 0;
    }

    // The pinned storage object itself (normally the byte[] the buffer wraps). PinnedBuffer is a
    // per-access VIEW — e.g. @string.buffer creates a fresh one per unsafe.StringData call — so
    // pointer identity (ж equality/hashing) must compare this canonical target, not the view.
    internal object? PinnedTarget => m_handle.IsAllocated ? m_handle.Target : null;

    public nint Length => m_len;

    public Span<byte> ꓸꓸꓸ => ToSpan();

    // A detached copy, charged like every other IArray.Source (slice<T>.Source, array<T>.ToArray).
    public byte[] Source => AllocationCounter.CopyOf<byte>(ToSpan());

    Array IArray.Source => Source;

    public unsafe ref byte this[nint index]
    {
        get
        {
            if (index < 0 || index >= m_len)
                throw new IndexOutOfRangeException();

            return ref Pointer[index];
        }
    }

    // IArray implementation allows PinnedBuffer to be used as
    // an indexed array pointer reference, see ptr constructor:
    // ж(IArray array, int index)
    unsafe object? IArray.this[nint index]
    {
        get => this[index];
        set
        {
            if (index < 0 || index >= m_len)
                throw new IndexOutOfRangeException();

            Pointer[index] = value switch
            {
                byte val => val,
                NilType => 0,
                null => 0,
                _ => throw new ArgumentException("Invalid value type.")
            };
        }
    }

    public slice<byte> this[Range range] => ToSpan()[range];

    public slice<byte> Slice(int start, int length) => ToSpan().Slice(start, length);

    public slice<byte> Slice(nint start, nint length) => ToSpan().Slice((int)start, (int)length);

    public unsafe Span<byte> ToSpan()
    {
        return new Span<byte>(Pointer, m_len);
    }

    public IEnumerator<(nint, byte)> GetEnumerator()
    {
        byte[] array = Source;
        nint index = 0;

        foreach (byte item in array)
            yield return (index++, item);
    }

    IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
    {
        return ((IEnumerable<byte>)Source).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Source.GetEnumerator();
    }

    public object Clone()
    {
        // Re-pin the target rather than copying the handle struct: two buffers over one handle
        // slot would both finalize it, and the second Free lands on whatever the runtime re-issued
        // the slot to — GC handle-table corruption. A pinned object cannot move, so the fresh pin
        // answers the same address.
        return Of(PinnedTarget, m_len);
    }

    public static implicit operator PinnedBuffer(byte[] array)
    {
        return Of(array);
    }

    public static implicit operator Span<byte>(PinnedBuffer buffer)
    {
        return buffer.ToSpan();
    }

    public static unsafe implicit operator byte*(PinnedBuffer buffer)
    {
        return buffer.Pointer;
    }

    public static implicit operator PinnedBuffer(@string src)
    {
        return Of(src, src.Length);
    }
}

/// <summary>
/// A pin made while the pin-extent index is on: it enters the index at construction and carries its entry, so it leaves
/// without a lookup. Only <see cref="PinnedBuffer.Of(byte[])"/> makes one, so a process that never switches the index on
/// never allocates the extra field.
/// </summary>
internal sealed class IndexedPinnedBuffer : PinnedBuffer
{
    // The entry, or s_none when the pin holds no bytes or has left.
    private object? m_extent;
    private static readonly object s_none = new();

    internal IndexedPinnedBuffer(byte[] array) : base(array) => Enter();

    internal IndexedPinnedBuffer(object? src, int len) : base(src, len) => Enter();

    private void Enter()
    {
        object? extent = null;

        if (PinExtents.Current is { } index && TryExtent(out nuint start, out nuint end))
            extent = index.Add(this, start, end);

        m_extent = extent ?? s_none;
    }

    // Entered at construction, so the backfill has nothing to do.
    internal override void EnterIndex() { }

    private protected override void LeaveIndex()
    {
        object? extent = Interlocked.Exchange(ref m_extent, s_none);

        if (extent is not null && extent != s_none)
            PinExtents.Current?.Remove(extent);
    }
}
