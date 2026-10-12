// Copyright � 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using go.golib;

namespace go;

/// <summary>
/// A stack-only Go slice view over a <see cref="Span{T}"/>. Unlike <see cref="slice{T}"/>, an
/// <see cref="sslice{T}"/> cannot escape the current stack frame or be boxed, stored, or captured.
/// </summary>
public ref struct sslice<T>
{
    // The full accessible window establishes capacity; m_length establishes the Go slice length.
    private Span<T> m_span;
    private readonly int m_length;

    public sslice(Span<T> source)
    {
        m_span = source;
        m_length = source.Length;
    }

    private sslice(Span<T> source, int length)
    {
        m_span = source;
        m_length = length;
    }

    public nint Length => m_length;

    public nint Capacity => m_span.Length;

    public nint Available => m_span.Length - m_length;

    public Span<T> ToSpan()
    {
        return m_span[..m_length];
    }

    // Spread operator, as slice<T>'s is. A pass-through `f(x, v...)` of a stack-view variadic (REC-C,
    // section A) hands the callee's params Span<T> the SAME storage, as Go passes the pack's header.
    // Spelled with escapes: this file is not UTF-8 (its header's copyright sign is one Latin-1 byte),
    // so a raw glyph here would be read through whatever fallback code page the compiler picks.
    // It clears golib's spread record (SpreadRecord.cs): a view's window carries no capacity the record
    // could vouch for, so an escaping callee copies it rather than take a stale slice of the same storage.
    public Span<T> \uA4F8\uA4F8\uA4F8
    {
        get
        {
            SpreadRecord<T>.Clear();
            return ToSpan();
        }
    }

    public Span<T> ToCapacitySpan()
    {
        return m_span;
    }

    public T[] ToArray()
    {
        return AllocationCounter.CopyOf<T>(ToSpan());
    }

    public slice<T> ToSlice()
    {
        return new slice<T>(ToSpan());
    }

    public ref T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            return ref m_span[index];
        }
    }

    public ref T this[nint index]
    {
        get
        {
            if (index < 0 || index >= m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            return ref m_span[(int)index];
        }
    }

    // An UNSIGNED index is checked here, before any narrowing: `(nint)index` of a value at or above
    // 2^63 reads negative and would report Go's signed text ([-1]) where Go reports the unsigned value
    // with the length (goPanicIndexU, boundsError.signed false).
    public ref T this[ulong index]
    {
        get
        {
            if (index >= (ulong)m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            return ref this[(nint)index];
        }
    }

    public sslice<T> this[Range range]
    {
        get
        {
            nint low = range.Start.IsFromEnd ? m_length - range.Start.Value : range.Start.Value;
            nint high = range.End.IsFromEnd ? m_length - range.End.Value : range.End.Value;
            return Reslice(low, high);
        }
    }

    public sslice<T> Slice(int start, int length)
    {
        return Reslice(start, start + length);
    }

    public sslice<T> Slice(nint start, nint length)
    {
        return Reslice(start, start + length);
    }

    // Go's 2-index s[low:high]: high against the capacity, then low against high (goPanicSliceAcap, goPanicSliceB),
    // as slice<T>.Reslice(low, high) does. The 3-index Reslice below reports the 3-index shapes.
    public sslice<T> Reslice(nint low, nint high)
    {
        if ((nuint)high > (nuint)Capacity || (nuint)low > (nuint)high)
            throw RuntimeErrorPanic.SliceBoundsOutOfRange(low, high, Capacity);

        return Reslice(low, high, Capacity);
    }

    public sslice<T> Reslice(nint low, nint high, nint max)
    {
        if (low < 0 || high < low || max < high || max > m_span.Length)
            throw RuntimeErrorPanic.SliceBoundsOutOfRange(low, high, max, m_span.Length);

        return new sslice<T>(m_span.Slice((int)low, (int)(max - low)), (int)(high - low));
    }

    public nint IndexOf(in T item)
    {
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;

        for (int i = 0; i < m_length; i++)
        {
            if (comparer.Equals(m_span[i], item))
                return i;
        }

        return -1;
    }

    public bool Contains(in T item)
    {
        return IndexOf(item) >= 0;
    }

    public void CopyTo(Span<T> destination)
    {
        ToSpan().CopyTo(destination);
    }

    public void CopyTo(T[] destination, int destinationIndex)
    {
        ToSpan().CopyTo(destination.AsSpan(destinationIndex));
    }

    /// <summary>
    /// Appends in place when the existing stack-backed capacity is sufficient. When it is not,
    /// <paramref name="result"/> is default and the caller must materialize a <see cref="slice{T}"/>.
    /// </summary>
    public bool TryAppend(ReadOnlySpan<T> elems, out sslice<T> result)
    {
        if (elems.Length > Available)
        {
            result = default;
            return false;
        }

        elems.CopyTo(m_span.Slice(m_length));
        result = new sslice<T>(m_span, m_length + elems.Length);
        return true;
    }

    public void Clear()
    {
        ToSpan().Clear();
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(ToSpan());
    }

    public static implicit operator sslice<T>(Span<T> source)
    {
        return new sslice<T>(source);
    }

    public static implicit operator Span<T>(sslice<T> source)
    {
        // The same window the spread getter hands out, so the same clear (SpreadRecord.cs).
        SpreadRecord<T>.Clear();
        return source.ToSpan();
    }

    public static implicit operator slice<T>(sslice<T> source)
    {
        return source.ToSlice();
    }

    public ref struct Enumerator
    {
        private readonly Span<T> m_span;
        private int m_index;

        internal Enumerator(Span<T> span)
        {
            m_span = span;
            m_index = -1;
        }

        public (nint, T) Current
        {
            get
            {
                if ((uint)m_index >= (uint)m_span.Length)
                    throw new InvalidOperationException("sslice enumeration is not positioned on an element.");

                return (m_index, m_span[m_index]);
            }
        }

        public bool MoveNext()
        {
            return ++m_index < m_span.Length;
        }
    }
}

public static class SSliceExtensions
{
    /// <summary>
    /// Creates a stack-only slice view over a span. The view shares the span's storage and does not allocate.
    /// </summary>
    public static sslice<T> sslice<T>(this Span<T> source, nint low = -1, nint high = -1, nint max = -1)
    {
        // The viewing prologue consumes the spread record (SpreadRecord.cs). A view hands its window on only
        // through its spread getter or its Span<T> conversion, which clear the record themselves; this clear
        // ends the record's hold on the spread array, and leaves no record for a span made by other means.
        SpreadRecord<T>.Clear();

        if (low == -1 && high == -1 && max == -1)
            return new sslice<T>(source);

        nint capacity = source.Length;
        low = low == -1 ? 0 : low;
        high = high == -1 ? capacity : high;
        max = max == -1 ? capacity : max;

        return new sslice<T>(source).Reslice(low, high, max);
    }
}
