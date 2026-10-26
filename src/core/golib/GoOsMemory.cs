// GoOsMemory.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;

namespace go;

/// <summary>
/// The OS memory the Go RUNTIME owns: every range its own allocation primitives obtained
/// (<c>sysAllocOS</c>, <c>sysReserveOS</c>) and have not yet released (<c>sysFreeOS</c>).
/// </summary>
/// <remarks>
/// <para>
/// WHY OWNERSHIP IS THE KEY (G's ruling, 2026-10-19). Go draws the same line itself, between the Go heap and
/// C memory, and its cgo pointer-passing rules forbid storing a Go pointer in memory the runtime did not
/// allocate. So a pointer word in memory the runtime does NOT own is a foreign address, and
/// <see cref="builtin.NativeArrayPointer{T}"/> reads it as one (<see cref="NativePointerArrayBox{E}"/>), while
/// pointer words in runtime-owned OS memory are Go pointers (the arena index, the page allocator's chunks), whose
/// elements live in a managed shadow (<see cref="ShadowArrayBox{T}"/>).
/// </para>
/// <para>
/// Every OS mapping primitive registers what it maps and unregisters what it releases, so a recycled address
/// reads as foreign again: linux's <c>sysMmap</c> / <c>sysMunmap</c>, darwin's <c>mmap</c> / <c>munmap</c>, and
/// windows' <c>sysAllocOS</c> / <c>sysReserveOS</c> / <c>sysFreeOS</c> (a decommit keeps its reservation, so
/// <c>sysUnusedOS</c> does not unregister). Go frees subranges, so a release splits the range it lands in. A
/// repoguard census (TestOsMemoryRegistrationCensus) names every runtime caller of the OS mapping calls per GOOS,
/// so a new route is classified before it lands.
/// </para>
/// <para>
/// SHADOW STORES. A managed shadow is keyed by its block's address and element type and lives until the range
/// is released, so two pointers to one runtime-owned block see one set of elements, as two Go pointers to one
/// block see one memory, and a block the runtime allocates afresh reads zero, as fresh OS memory does.
/// </para>
/// <para>
/// The registry holds a handful of entries (the runtime reserves its arenas in large ranges), so a lookup is a
/// scan under a lock; it is consulted only where a pointer to an array over native memory is minted.
/// </para>
/// </remarks>
internal static class GoOsMemory
{
    private static readonly object s_lock = new();

    // Disjoint [Base, End) ranges, in registration order.
    private static readonly List<(nuint Base, nuint End)> s_ranges = [];

    // The managed shadow of each runtime-owned block a pointer to array was minted over: (address, element type)
    // -> array<T>. Dropped with the range that holds it.
    private static readonly Dictionary<(nuint Address, Type Element), object> s_shadows = [];

    /// <summary>Records [<paramref name="address"/>, <paramref name="address"/> + <paramref name="length"/>) as runtime-owned.</summary>
    internal static void Register(nuint address, nuint length)
    {
        if (address == 0 || length == 0)
            return;

        lock (s_lock)
        {
            // A reservation the kernel hands out again after a release can overlap nothing still registered, but
            // a range is subtracted first anyway, so the list stays disjoint whatever order the calls arrive in.
            subtract(address, address + length);
            s_ranges.Add((address, address + length));
        }
    }

    /// <summary>
    /// Releases [<paramref name="address"/>, <paramref name="address"/> + <paramref name="length"/>): it reads as
    /// foreign from here on, and the shadows inside it are dropped.
    /// </summary>
    internal static void Unregister(nuint address, nuint length)
    {
        if (address == 0 || length == 0)
            return;

        lock (s_lock)
        {
            subtract(address, address + length);

            List<(nuint, Type)>? dropped = null;

            foreach ((nuint Address, Type Element) key in s_shadows.Keys)
            {
                if (key.Address >= address && key.Address - address < length)
                    (dropped ??= []).Add(key);
            }

            if (dropped is not null)
            {
                foreach ((nuint, Type) key in dropped)
                    s_shadows.Remove(key);
            }
        }
    }

    /// <summary>Whether <paramref name="address"/> lies in memory the Go runtime owns.</summary>
    internal static bool Owns(nuint address)
    {
        lock (s_lock)
            return ownsLocked(address);
    }

    /// <summary>
    /// The managed shadow of the runtime-owned block at <paramref name="address"/>, as <paramref name="length"/>
    /// elements: the one already minted over it, or a fresh zeroed one; null over memory the runtime does not own,
    /// which has no shadow (its elements are data a zeroed store would misread).
    /// </summary>
    internal static array<T>? Shadow<T>(nuint address, nint length)
    {
        lock (s_lock)
        {
            if (!ownsLocked(address))
                return null;

            (nuint, Type) key = (address, typeof(T));

            // A view no longer than the store is a window of it, so it has the view's own N.
            if (s_shadows.TryGetValue(key, out object? existing) && existing is array<T> shadow && shadow.Length >= length)
                return shadow.Length == length ? shadow : array<T>.Alias(new slice<T>(shadow.Source), length);

            // A first mint, or a longer view of a block minted shorter before: the elements already stored carry
            // over, and the longer store is the block's shadow from here on. A box minted over the shorter store
            // keeps it, so a block viewed at two lengths shares its elements only from the longer mint on; no
            // runtime site views one block at two lengths.
            array<T> grown = new(length);

            if (existing is array<T> shorter)
                Array.Copy(shorter.Source, grown.Source, shorter.Length);

            s_shadows[key] = grown;
            return grown;
        }
    }

    private static bool ownsLocked(nuint address)
    {
        foreach ((nuint Base, nuint End) range in s_ranges)
        {
            if (address >= range.Base && address < range.End)
                return true;
        }

        return false;
    }

    // Removes [low, high) from every registered range, splitting a range it lands inside.
    private static void subtract(nuint low, nuint high)
    {
        for (int i = s_ranges.Count - 1; i >= 0; i--)
        {
            (nuint Base, nuint End) range = s_ranges[i];

            if (range.End <= low || range.Base >= high)
                continue;

            s_ranges.RemoveAt(i);

            if (range.Base < low)
                s_ranges.Add((range.Base, low));

            if (range.End > high)
                s_ranges.Add((high, range.End));
        }
    }
}
