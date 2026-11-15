// builtin.InlineArrayCopy.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go.golib;

namespace go;

public static partial class builtin
{
    /// <summary>
    /// Go's <c>copy(x.f[low:high], src)</c> where <c>f</c> is an array held inline (a <c>go.arrays</c> member): the
    /// elements of <paramref name="src"/> written into the window of the array's own storage, read through a reference
    /// for the copy's duration. No pin and no box: the slice Go writes never escapes the copy, so nothing outlives the
    /// statement, as in Go. An escaping slice of an inline array is <see cref="ж{T}.ΔInlineSlice{TElem}"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static nint copy<TArray, T>(ref TArray dst, nint low, nint high, slice<T> src) where TArray : struct
    {
        Span<T> window = InlineArrayWindow<TArray, T>(ref dst, low, high);
        int count = (int)Math.Min(window.Length, src.Length);

        src.ToSpan()[..count].CopyTo(window);

        return count;
    }

    /// <summary>
    /// Go's <c>copy(dst, x.f[low:high])</c> where <c>f</c> is an array held inline: the window of the array's own
    /// storage read into <paramref name="dst"/>, with no pin and no box (see the overload above).
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static nint copy<T, TArray>(slice<T> dst, ref TArray src, nint low, nint high) where TArray : struct
    {
        Span<T> window = InlineArrayWindow<TArray, T>(ref src, low, high);
        int count = (int)Math.Min(window.Length, dst.Length);

        window[..count].CopyTo(dst.ToSpan());

        return count;
    }

    // The window [low:high] of an inline array's storage, checked as Go checks a slice of an array: 0 <= low <= high <=
    // len, else Go's own bounds panic.
    private static Span<T> InlineArrayWindow<TArray, T>(ref TArray array, nint low, nint high) where TArray : struct
    {
        int length = InlineArrayShape<TArray, T>.Length;

        if (length < 0)
            throw new PanicException($"inline array copy: {typeof(TArray).Name} is not an inline array of {typeof(T).Name}");

        if (low < 0 || high < low || high > length)
            throw RuntimeErrorPanic.LengthSliceBoundsOutOfRange(low, high, length);

        return MemoryMarshal.CreateSpan(ref Unsafe.As<TArray, T>(ref array), length)[(int)low..(int)high];
    }
}
