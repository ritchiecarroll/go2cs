// GoForeignWords.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;

namespace go;

/// <summary>
/// The values the foreign pointer-array kind (<see cref="NativePointerArrayBox{E}"/>) minted from words in memory
/// the Go runtime did not allocate, kept so that a store into one is refused by name.
/// </summary>
/// <remarks>
/// <para>
/// PREMISE (G's ruling, 2026-10-19): memory a producer hands to Go -- a system call's result, a DNS record, an
/// address list -- is immutable until the producer releases it. So the words read once are the words, and a value
/// minted from one is an exact read for as long as the memory lives.
/// </para>
/// <para>
/// WHY A CHECKPOINT. golib cannot intercept a store: the element door and a slice both hand out <c>ref T</c>, and
/// a C# <c>ref</c> sees no write. So each minted value is kept beside the place it was handed out to -- an element
/// slot of the array, or the managed copy a slice of it reads into -- and compared again (a) at every later access
/// through a foreign array over the same word, and (b) when the producer releases the memory
/// (<see cref="GoForeignMemory.Release"/>). A changed value is refused by name, so a store is refused no later
/// than the memory's release; it never reaches the word, and it is never silently lost.
/// </para>
/// <para>
/// A holder is kept weakly: a slice nobody can reach can take no more stores, so its record goes with it.
/// </para>
/// </remarks>
internal static class GoForeignWords
{
    /// <summary>A holder whose current value for one word is compared against what was minted into it.</summary>
    internal interface ISlot
    {
        /// <summary>Whether the value handed out has been replaced since it was minted.</summary>
        bool Changed { get; }
    }

    // One minted value: its holder (an ISlot, or the object[] a slice reads into, at Index), and, for an array
    // holder, the value minted there.
    private readonly record struct Entry(WeakReference<object> Holder, int Index, object? Minted);

    private static readonly object s_lock = new();

    // word address -> the values minted from it
    private static readonly Dictionary<nuint, List<Entry>> s_words = [];

    // Records swept of dead holders every this many mints.
    private const int SweepInterval = 4096;
    private static int s_mintsSinceSweep;

    /// <summary>Records an element slot minted from the word at <paramref name="word"/>.</summary>
    internal static void Minted(nuint word, ISlot slot)
    {
        lock (s_lock)
            add(word, new Entry(new WeakReference<object>(slot), 0, null));
    }

    /// <summary>
    /// Records <paramref name="elements"/>[<paramref name="index"/>], minted from the word at <paramref name="word"/>
    /// into the managed copy a slice reads.
    /// </summary>
    internal static void Minted(nuint word, object[] elements, int index)
    {
        lock (s_lock)
            add(word, new Entry(new WeakReference<object>(elements), index, elements[index]));
    }

    /// <summary>Refuses by name a store into any value minted from the words in [<paramref name="low"/>, <paramref name="high"/>).</summary>
    internal static void Verify(nuint low, nuint high)
    {
        lock (s_lock)
        {
            for (nuint word = low; word < high; word += (nuint)IntPtr.Size)
            {
                if (s_words.TryGetValue(word, out List<Entry>? entries) && firstStore(word, entries, prune: true) is { } refusal)
                    throw refusal;
            }
        }
    }

    /// <summary>
    /// The producer released [<paramref name="address"/>, <paramref name="address"/> + <paramref name="length"/>):
    /// every value minted from it is compared a last time and forgotten, and the first store found is refused.
    /// </summary>
    internal static void Release(nuint address, nuint length)
    {
        PanicException? refusal = null;

        lock (s_lock)
        {
            List<nuint>? released = null;

            foreach (KeyValuePair<nuint, List<Entry>> pair in s_words)
            {
                if (pair.Key < address || pair.Key - address >= length)
                    continue;

                refusal ??= firstStore(pair.Key, pair.Value, prune: false);
                (released ??= []).Add(pair.Key);
            }

            if (released is not null)
            {
                foreach (nuint word in released)
                    s_words.Remove(word);
            }
        }

        // Forgotten first: the memory is gone, so the refusal is the last word on it.
        if (refusal is not null)
            throw refusal;
    }

    private static void add(nuint word, Entry entry)
    {
        if (!s_words.TryGetValue(word, out List<Entry>? entries))
            s_words.Add(word, entries = []);

        entries.Add(entry);

        if (++s_mintsSinceSweep < SweepInterval)
            return;

        s_mintsSinceSweep = 0;
        List<nuint>? empty = null;

        foreach (KeyValuePair<nuint, List<Entry>> pair in s_words)
        {
            pair.Value.RemoveAll(static candidate => !candidate.Holder.TryGetTarget(out _));

            if (pair.Value.Count == 0)
                (empty ??= []).Add(pair.Key);
        }

        if (empty is not null)
        {
            foreach (nuint key in empty)
                s_words.Remove(key);
        }
    }

    private static PanicException? firstStore(nuint word, List<Entry> entries, bool prune)
    {
        PanicException? refusal = null;

        for (int i = entries.Count - 1; i >= 0; i--)
        {
            Entry entry = entries[i];

            if (!entry.Holder.TryGetTarget(out object? holder))
            {
                if (prune)
                    entries.RemoveAt(i);

                continue;
            }

            bool changed = holder switch
            {
                ISlot slot => slot.Changed,
                object[] elements => !ReferenceEquals(elements[entry.Index], entry.Minted),
                _ => false
            };

            if (changed)
                refusal ??= StoreRefused(word, holder is ISlot ? "an element of the array" : "a slice of the array");
        }

        return refusal;
    }

    /// <summary>The refusal of a store made through <paramref name="through"/> into the word at <paramref name="word"/>.</summary>
    internal static PanicException StoreRefused(nuint word, string through) =>
        new($"native pointer array: the word at 0x{word:x} was stored into through {through}; it is memory the Go " +
            "runtime did not allocate, read as its producer's and immutable until the producer releases it, so " +
            "the store is refused rather than lost (Go forbids a Go pointer there, and the C memory is its producer's)");
}

/// <summary>
/// The release seam for memory a producer outside the Go runtime handed to Go code: a system call's result
/// buffer, a DNS record list, an address list.
/// </summary>
public static class GoForeignMemory
{
    /// <summary>
    /// Called by a producer's release hand-own (DnsRecordListFree, FreeAddrInfoW, any transcription seam) before
    /// the memory at [<paramref name="address"/>, <paramref name="address"/> + <paramref name="length"/>) is freed:
    /// a store Go code made into a pointer read from it is refused here by name, if no earlier access refused it.
    /// </summary>
    public static void Release(nuint address, nuint length)
    {
        if (address != 0 && length != 0)
            GoForeignWords.Release(address, length);
    }
}
