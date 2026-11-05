// StringLiteralCache.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace go;

/// <summary>
/// The one-conversion-per-literal cache behind <c>implicit operator @string(string)</c>.
/// </summary>
/// <remarks>
/// <para>
/// Go keeps a string literal in RODATA, so evaluating <c>"hello"</c> costs nothing. A C# string
/// literal is UTF-16 and a Go string is UTF-8, so converting one transcodes and allocates on every
/// evaluation. This cache makes the conversion of a C# <b>literal</b> happen once per process: the
/// first evaluation transcodes and stores the result, every later one returns the stored
/// <see cref="@string"/>, which is immutable and therefore safe to share (exactly as a hoisted
/// <c>static readonly @string</c> field shares one instance across every use).
/// </para>
/// <para>
/// <b>Only literals are admitted.</b> A literal of a non-collectible module is a FROZEN object on the
/// non-GC heap: it never moves and never dies, and <see cref="GC.GetGeneration(object)"/> reports
/// <see cref="int.MaxValue"/> for it (JIT and Native AOT, measured). So its address is a stable hash
/// key and holding it strongly leaks nothing. A run-time string (gen 0-2), and a literal of a
/// collectible AssemblyLoadContext (not frozen), is converted exactly as before and never stored, so
/// the table is bounded by the set of distinct literals the program evaluates, like Go's RODATA. A
/// lookup compares references, never contents, so a run-time string equal to a cached literal is a
/// miss that converts normally (correct, and what Go's <c>string(b)</c> does too).
/// </para>
/// <para>
/// Layout: open addressing with linear probing over inline (key, value) slots, kept at most half full
/// and grown by copy. Reads take no lock: a slot's value is written before its key is published with
/// a release store, and readers acquire the key first; a grown table is published whole. Inserts take
/// one lock. A hit is two independent static loads, a multiply-shift hash, an acquire load, a
/// reference compare and one 16-byte copy through the returned ref, with no frame of its own to zero.
/// </para>
/// </remarks>
internal static class StringLiteralCache
{
    private struct Slot
    {
        public string? Key;
        public @string Value;
    }

    private const int InitialLog2 = 8;

    // The table is two statics, not one object, so a hit loads both at once instead of a chain. A
    // grown table is still published whole: s_slots is filled, then published, then s_shift. A reader
    // acquires s_shift BEFORE it reads s_slots, so it sees a table at least as new as its shift; and
    // tables only grow, so an older shift indexes a newer table in bounds (it probes a wrong slot and
    // misses, and Slow re-derives the shift from the table it reads). s_count is guarded by s_lock.
    private static Slot[] s_slots = NewSlots(InitialLog2);
    private static int s_shift = 64 - InitialLog2;
    private static int s_count;
    private static readonly Lock s_lock = new();

    private static Slot[] NewSlots(int log2) => new Slot[1 << log2];

    // Where a small conversion that is not cached lands, so every path returns a ref and the caller
    // copies once. Read through the returned ref at once: this thread's next one overwrites it.
    [ThreadStatic]
    private static @string t_uncached;

    // The largest backing t_uncached may hold, so it never keeps a large one alive (see Uncached).
    internal const int UncachedSlotLimit = 256;

    // Fibonacci hashing of the object address: frozen literals are laid out consecutively, and a
    // multiply by 2^64/phi spreads an arithmetic progression evenly over the top bits. The address is
    // taken by value, through the first character (a fixed offset into the object): reinterpreting
    // the reference with Unsafe.As takes the PARAMETER's address and spills it to the frame.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int Index(string s, int shift) =>
        (int)((ulong)Unsafe.AsPointer(ref Unsafe.AsRef(in s.GetPinnableReference())) * 0x9E3779B97F4A7C15UL >> shift);

    /// <summary>
    /// Converts a C# string to <see cref="@string"/>, returning the shared conversion when the string
    /// is a literal already converted once.
    /// </summary>
    /// <remarks>
    /// Returned by ref so the hit and the miss merge on an address, not on a <see cref="@string"/>
    /// temporary the caller's frame would zero on every call. Copy it out before converting again.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly @string Get(string? value)
    {
        if (value is not null)
        {
            int shift = Volatile.Read(ref s_shift);
            ref Slot slot = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(s_slots), Index(value, shift));

            if (ReferenceEquals(Volatile.Read(ref slot.Key), value))
                return ref slot.Value;
        }

        return ref Slow(value);
    }

    // The probe continuation. Kept free of @string locals (see Miss).
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ref readonly @string Slow(string? value)
    {
        if (value is not null)
        {
            Slot[] slots = s_slots;
            int mask = slots.Length - 1;
            int i = Index(value, 64 - BitOperations.Log2((uint)slots.Length));
            string? key;

            while ((key = Volatile.Read(ref slots[i].Key)) is not null)
            {
                if (ReferenceEquals(key, value))
                    return ref slots[i].Value;

                i = (i + 1) & mask;
            }
        }

        return ref Miss(value);
    }

    // Today's conversion, byte for byte: AllocationCounter.Utf8ToBytes is Encoding.UTF8.GetBytes
    // (a lone surrogate becomes EF BF BD, null becomes ""), charged to AllocationCounter as before.
    // No @string local lives here or in Slow: a frame the JIT zeroes with a 256-bit store, followed
    // by legacy-SSE ReadyToRun code (UTF-8 transcoding with tiering off), measured ~50 ns of AVX/SSE
    // transition penalty per call on Haswell.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ref readonly @string Miss(string? value)
    {
        byte[] bytes = AllocationCounter.Utf8ToBytes(value);

        if (value is null || GC.GetGeneration(value) != int.MaxValue)
            return ref Uncached(bytes);

        return ref Insert(value, bytes);
    }

    // A conversion that is not cached lands where the caller can copy it from: a small one in this
    // thread's slot, which allocates nothing; a large one in a box of its own, so the slot never keeps
    // a large backing alive after the caller has dropped it.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ref readonly @string Uncached(byte[] bytes)
    {
        if (bytes.Length > UncachedSlotLimit)
            return ref new StrongBox<@string>(new @string(bytes)).Value;

        t_uncached = new @string(bytes);
        return ref t_uncached;
    }

    // Returns a ref to the slot's value: a published slot is never rewritten, and a table a grow
    // replaced keeps its values, so the ref stays valid.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ref readonly @string Insert(string key, byte[] bytes)
    {
        @string value = new(bytes);

        lock (s_lock)
        {
            Slot[] slots = s_slots;
            ref Slot slot = ref Find(slots, key, out bool found);

            // Another thread converted this literal first: return ITS value, so every evaluation of
            // one literal yields one backing array, as a hoisted field does.
            if (found)
                return ref slot.Value;

            if ((s_count + 1) * 2 > slots.Length)
            {
                int log2 = BitOperations.Log2((uint)slots.Length) + 1;
                Slot[] grown = NewSlots(log2);

                foreach (Slot old in slots)
                {
                    if (old.Key is null)
                        continue;

                    Find(grown, old.Key, out _) = old;
                }

                slot = ref Find(grown, key, out _);
                slot.Value = value;
                slot.Key = key;
                Volatile.Write(ref s_slots, grown);
                Volatile.Write(ref s_shift, 64 - log2);
            }
            else
            {
                slot.Value = value;
                Volatile.Write(ref slot.Key, key);
            }

            s_count++;

            return ref slot.Value;
        }
    }

    private static ref Slot Find(Slot[] slots, string key, out bool found)
    {
        int mask = slots.Length - 1;
        int i = Index(key, 64 - BitOperations.Log2((uint)slots.Length));

        while (true)
        {
            ref Slot slot = ref slots[i];
            string? current = Volatile.Read(ref slot.Key);

            if (current is null)
            {
                found = false;
                return ref slot;
            }

            if (ReferenceEquals(current, key))
            {
                found = true;
                return ref slot;
            }

            i = (i + 1) & mask;
        }
    }

    /// <summary>Number of literals converted and cached so far (diagnostics and tests).</summary>
    internal static int Count => Volatile.Read(ref s_count);

    /// <summary>Whether this exact string instance has a cached conversion (diagnostics and tests).</summary>
    internal static bool Contains(string value)
    {
        Find(s_slots, value, out bool found);
        return found;
    }
}
