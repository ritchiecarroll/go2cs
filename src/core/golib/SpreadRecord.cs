// SpreadRecord.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace go;

// The slice behind the last spread on this thread (COORD ruling 2026-10-12, route (a)). In Go, `f(xs...)`
// passes the slice ITSELF, so a callee that keeps its pack (`return xs`, a store, a capture) shares the
// caller's backing array and capacity. A converted callee receives the pack as a params Span<T>, which
// has no capacity and cannot be kept, so its escaping prologue (`var xs = xsʗp.slice();`) used to copy.
//
// slice<T>'s spread getter records the slice here, and the escaping prologue returns it when the pack IS
// its window: the same first element and the same length. Anything else is copied, as before. The record
// lives from one spread to one prologue:
//
//   - slice<T>.ꓸꓸꓸ                          records the slice
//   - Span<T>.slice()   (escaping prologue)  takes it, on a match
//   - Span<T>.sslice()  (viewing prologue)   clears it
//   - sslice<T>.ꓸꓸꓸ, sslice -> Span<T>,      clear it: each hands out a window over storage the record
//     array<T>.ꓸꓸꓸ                           may also cover, with a capacity the record does not know
//
// The match cannot be fooled by the record's own slice; it could be fooled only by a DIFFERENT span over
// the same first element with the same length, which a stale record meets when a window with another
// capacity reaches the prologue by another route. Every route by which converted code makes such a window
// clears the record first (the last row above). The viewing prologue's clear is not needed for that (a view
// hands its window on only through the last row), but it ends the record's hold on the spread array. The census of golib and the hand-owned files is in
// the conversion-strategy reference ("Variadic functions"). The record is per thread and per element
// type, so neither another goroutine nor a spread of another type can disturb it.
//
// A native-backed slice (its span is not over m_array) and a slice of a zero-size type (its span is fresh
// storage, see slice<T>.ToSpan) are never recorded: their packs are copied, as before.
internal static class SpreadRecord<T>
{
    [ThreadStatic] private static slice<T> t_last;
    [ThreadStatic] private static bool t_valid;

    // A pooled thread starts its next goroutine with no record (and no hold on the last spread array).
    private static readonly bool s_reset = go.golib.GoroutineThreadState.Register(static () => Clear());

    internal static Span<T> Record(slice<T> spread)
    {
        Span<T> view = spread.ToSpan();

        if (spread.IsNativeBacked || GoZeroSizeFacts<T>.IsZeroSize)
        {
            Clear();
            return view;
        }

        t_last = spread;
        t_valid = true;

        return view;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Clear()
    {
        if (t_valid)
        {
            t_valid = false;
            t_last = default;
        }
    }

    internal static bool TryTake(Span<T> pack, out slice<T> spread)
    {
        if (!t_valid)
        {
            spread = default;
            return false;
        }

        slice<T> last = t_last;
        t_valid = false;
        t_last = default;

        T[] array = last.m_array;

        if (array is not null && pack.Length == last.Length &&
            Unsafe.AreSame(ref MemoryMarshal.GetReference(pack), ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), last.Low)))
        {
            spread = last;
            return true;
        }

        spread = default;
        return false;
    }
}
