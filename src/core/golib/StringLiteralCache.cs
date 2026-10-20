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
/// one lock. A hit is one table load, a multiply-shift hash, an acquire load and a reference compare
/// (about 0.5 ns over a hoisted field, measured).
/// </para>
/// </remarks>
internal static class StringLiteralCache
{
    private struct Slot
    {
        public string? Key;
        public @string Value;
    }

    private sealed class Table(int log2)
    {
        public readonly Slot[] Slots = new Slot[1 << log2];
        public readonly int Shift = 64 - log2;
        public int Count;
    }

    private const int InitialLog2 = 8;

    private static Table s_table = new(InitialLog2);
    private static readonly Lock s_lock = new();

    // Fibonacci hashing of the object address: frozen literals are laid out consecutively, and a
    // multiply by 2^64/phi spreads an arithmetic progression evenly over the top bits.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Index(string s, int shift) =>
        (int)((ulong)Unsafe.As<string, nint>(ref s) * 0x9E3779B97F4A7C15UL >> shift);

    /// <summary>
    /// Converts a C# string to <see cref="@string"/>, returning the shared conversion when the string
    /// is a literal already converted once.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static @string Get(string? value)
    {
        if (value is not null)
        {
            Table table = s_table;
            ref Slot slot = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(table.Slots), Index(value, table.Shift));

            if (ReferenceEquals(Volatile.Read(ref slot.Key), value))
                return slot.Value;
        }

        return Slow(value);
    }

    // The probe continuation. Kept free of @string locals (see Miss).
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static @string Slow(string? value)
    {
        if (value is not null)
        {
            Slot[] slots = s_table.Slots;
            int mask = slots.Length - 1;
            int i = Index(value, 64 - BitOperations.Log2((uint)slots.Length));
            string? key;

            while ((key = Volatile.Read(ref slots[i].Key)) is not null)
            {
                if (ReferenceEquals(key, value))
                    return slots[i].Value;

                i = (i + 1) & mask;
            }
        }

        return Miss(value);
    }

    // Today's conversion, byte for byte: AllocationCounter.Utf8ToBytes is Encoding.UTF8.GetBytes
    // (a lone surrogate becomes EF BF BD, null becomes ""), charged to AllocationCounter as before.
    // No @string local lives here or in Slow: a frame the JIT zeroes with a 256-bit store, followed
    // by legacy-SSE ReadyToRun code (UTF-8 transcoding with tiering off), measured ~50 ns of AVX/SSE
    // transition penalty per call on Haswell.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static @string Miss(string? value)
    {
        byte[] bytes = AllocationCounter.Utf8ToBytes(value);

        if (value is null || GC.GetGeneration(value) != int.MaxValue)
            return new @string(bytes);

        return Insert(value, bytes);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static @string Insert(string key, byte[] bytes)
    {
        @string value = new(bytes);

        lock (s_lock)
        {
            Table table = s_table;
            ref Slot slot = ref Find(table, key, out bool found);

            // Another thread converted this literal first: return ITS value, so every evaluation of
            // one literal yields one backing array, as a hoisted field does.
            if (found)
                return slot.Value;

            if ((table.Count + 1) * 2 > table.Slots.Length)
            {
                Table grown = new(64 - table.Shift + 1);

                foreach (Slot old in table.Slots)
                {
                    if (old.Key is null)
                        continue;

                    Find(grown, old.Key, out _) = old;
                }

                grown.Count = table.Count;
                slot = ref Find(grown, key, out _);
                slot.Value = value;
                slot.Key = key;
                grown.Count++;
                Volatile.Write(ref s_table, grown);
            }
            else
            {
                slot.Value = value;
                Volatile.Write(ref slot.Key, key);
                table.Count++;
            }
        }

        return value;
    }

    private static ref Slot Find(Table table, string key, out bool found)
    {
        Slot[] slots = table.Slots;
        int mask = slots.Length - 1;
        int i = Index(key, table.Shift);

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
    internal static int Count => s_table.Count;

    /// <summary>Whether this exact string instance has a cached conversion (diagnostics and tests).</summary>
    internal static bool Contains(string value)
    {
        Find(s_table, value, out bool found);
        return found;
    }
}
