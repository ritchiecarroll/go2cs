// GoPointerWords.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace go.golib;

/// <summary>
/// The table that stands in for the collector's tracing of an <c>unsafe.Pointer</c> stored into Go memory that holds a
/// WORD (docs/phase4/DESIGN-cgo-phase3-marshaling.md, 18.3 to 18.7): for each such slot, the referents its published
/// words name, so the word stays the referent's number and the referent stays alive while the word is published.
/// </summary>
/// <remarks>
/// <para>
/// In Go an <c>unsafe.Pointer</c> IS a word, and storage typed as holding pointers is traced. Here the word storage is a
/// managed array the CLR does not trace for pointers, so a Go pointer stored into it (`*p = v`, p a `*unsafe.Pointer`
/// over a Go word) would keep nothing. The store therefore writes the referent's NUMBER (unsafe.Pointer's pinning
/// projection: a pinned address, or an order token) and records an ENTRY, (number, referent), in a counted bag for the
/// slot. A load answers the referent whose entry matches the word. A value-type referent's box keeps its own pin-only
/// hold, so the number stays its address while the entry lives.
/// </para>
/// <para>
/// KEYS. A slot is the pinned ARRAY that holds the word (the storage object a box's pin holds still) and the byte offset
/// of the word in it. Two views of one word reach one object and one offset, whichever box each was derived from. The
/// table is a <see cref="ConditionalWeakTable{TKey,TValue}"/> keyed by that object: an ephemeron, so the entries live
/// exactly as long as the storage.
/// </para>
/// <para>
/// ONE LOCK PER STORAGE OBJECT covers a load's read of the word and its lookup, and a store's add, exchange and remove.
/// A load therefore never answers a number while the word's entry exists (the 18.6 finding 5 ABA hole cannot open), and
/// the bag invariant holds at every check: the entry for the slot's current word, when it was written here, never
/// reaches count 0.
/// </para>
/// <para>
/// THE NAMED LEAK (found 2026-11-17, widened 2026-11-20; df230c8d43): a published word whose referent reaches the box that pins the
/// destination's storage roots its own key, so the slot is retained until a word of the cycle is overwritten through a
/// store or cleared. Instances: a self word (c3), a mutual pair (c4), and a referent that holds the destination's box (c1).
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GoPointerWords
{
    private sealed class Entry(nuint number, object referent)
    {
        internal readonly nuint Number = number;
        internal readonly object Referent = referent;
        internal int Count = 1;
    }

    private sealed class Table
    {
        private static long s_lastId;

        // The order two tables are locked in, when a copy holds both.
        internal readonly long Id = Interlocked.Increment(ref s_lastId);

        // Slot byte offset -> the slot's bag. Guarded by this table's own lock.
        internal readonly Dictionary<nuint, List<Entry>> Slots = new();
    }

    private static readonly ConditionalWeakTable<object, Table> s_tables = new();

    // Set once, when the first table is made: until then every copy and clear door answers with one read of this flag.
    private static volatile bool s_anyTable;

    private static Table TableFor(object storage) => s_tables.GetValue(storage, static _ =>
    {
        s_anyTable = true;
        return new Table();
    });

    /// <summary>
    /// Where a word slot's storage is, for the word doors: Go memory that a pinned array holds (the table's slots), C
    /// memory alone, or neither.
    /// </summary>
    public enum SlotKind
    {
        /// <summary>A managed box, or a native box this table does not classify: the caller keeps its own semantics.</summary>
        NotAWord,

        /// <summary>A word of Go memory that a pinned array holds: the table's slots.</summary>
        GoMemory,

        /// <summary>A word of C memory alone: a number, never an entry.</summary>
        CMemory,

        /// <summary>A native box derived from Go storage that no pin holds still: no word to name.</summary>
        Unheld
    }

    /// <summary>Classifies the storage a native <c>*unsafe.Pointer</c> box names.</summary>
    public static SlotKind Classify<T>(ж<T> slot)
    {
        if (slot is not NativeBox<T> native || native.NativeAddress == 0)
            return SlotKind.NotAWord;

        if (((INativeRooted)native).IsGenuinelyNativeRoot)
            return SlotKind.CMemory;

        return TryLocate(native, out _, out _) ? SlotKind.GoMemory : SlotKind.Unheld;
    }

    // The slot a native box over Go memory names: the pinned array that holds its word, and the word's byte offset in it.
    private static bool TryLocate<T>(NativeBox<T> slot, out object storage, out nuint offset)
    {
        storage = null!;
        offset = 0;

        nuint address = slot.NativeAddress;

        if (NativeDerivation.HeldSource(slot.RetainedSource) is not { } held ||
            !held.TryHeldTarget(out object target, out nuint start, out nuint end) ||
            address < start || address + (nuint)IntPtr.Size > end)
            return false;

        storage = target;
        offset = address - start;
        return true;
    }

    private static NativeBox<T> GoWord<T>(ж<T> slot, out object storage, out nuint offset)
    {
        if (slot is not NativeBox<T> native || !TryLocate(native, out storage, out offset))
            throw RuntimeErrorPanic.RuntimeRaised("unsafe.Pointer word: the slot is not a word of Go memory a pin holds still");

        return native;
    }

    /// <summary>
    /// W1 over Go memory: the word, and the referent its entry names, or null when no entry names the word (a number, as
    /// a <c>uintptr</c> written there is).
    /// </summary>
    public static nuint Load<T>(ж<T> slot, out object? referent)
    {
        NativeBox<T> native = GoWord(slot, out object storage, out nuint offset);
        referent = null;

        // The word first, then the table: a word written by a store here was written after its table was made, so a
        // reader that sees it also sees the table.
        nuint word = native.ReadPointerWord();

        if (!s_tables.TryGetValue(storage, out Table? table))
            return word;

        lock (table)
        {
            word = native.ReadPointerWord();
            referent = Find(table, offset, word)?.Referent;
            return word;
        }
    }

    /// <summary>
    /// W2 over Go memory: publishes <paramref name="word"/> with its referent (null for a number), and answers the word it
    /// displaced. The add, the exchange and the remove happen under the storage's lock, in that order.
    /// </summary>
    public static nuint Store<T>(ж<T> slot, nuint word, object? referent) =>
        Swap(slot, word, referent, out _);

    /// <summary>W3's swap: <see cref="Store{T}"/> that also answers the displaced word's referent.</summary>
    public static nuint Swap<T>(ж<T> slot, nuint word, object? referent, out object? displacedReferent)
    {
        NativeBox<T> native = GoWord(slot, out object storage, out nuint offset);
        Table table = TableFor(storage);

        lock (table)
        {
            Add(table, offset, word, referent);
            nuint displaced = native.ExchangePointerWord(word);
            displacedReferent = Find(table, offset, displaced)?.Referent;
            Remove(table, offset, displaced, referent);
            return displaced;
        }
    }

    /// <summary>
    /// W3's compare-and-swap. The current word matches when it is <paramref name="oldWord"/>, or when the slot's entry for
    /// it names <paramref name="oldReferent"/> (an object pinned again can show a second number; defence in depth). Over a
    /// TOKEN with an entry, a number match also needs the entry's referent to be <paramref name="oldReferent"/>, because
    /// past the 2^29 id wrap two live boxes can share a token (design 18.7, item (ii), confirmed in review).
    /// </summary>
    public static bool CompareAndSwap<T>(ж<T> slot, nuint oldWord, object? oldReferent, nuint newWord, object? newReferent)
    {
        NativeBox<T> native = GoWord(slot, out object storage, out nuint offset);
        Table table = TableFor(storage);

        lock (table)
        {
            nuint current = native.ReadPointerWord();
            Entry? entry = Find(table, offset, current);
            bool matches;

            if (current == oldWord)
            {
                matches = !(ManagedPointerTokens.IsTaggedToken(current) && entry is not null && oldReferent is not null &&
                            !ReferenceEquals(entry.Referent, oldReferent));
            }
            else
            {
                matches = entry is not null && oldReferent is not null && ReferenceEquals(entry.Referent, oldReferent);
            }

            if (!matches)
                return false;

            Add(table, offset, newWord, newReferent);

            if (!native.CompareExchangePointerWord(current, newWord))
            {
                // Only a write that bypassed the table (a raw uintptr store) can move the word under the lock. Take back
                // what was added and answer as the comparison would have on the word now there.
                Remove(table, offset, newWord, null);
                return false;
            }

            Remove(table, offset, current, newReferent);
            return true;
        }
    }

    /// <summary>
    /// The memmove copy door: after <paramref name="n"/> bytes were copied from <paramref name="src"/> to
    /// <paramref name="dst"/>, each destination word slot drops its entries (an overwrite) and takes one count of the
    /// source slot's entry for the word now copied there. The source keeps its entries: a copy is not a move.
    /// </summary>
    public static void CopyEntries(object? dstCarried, nuint dst, object? srcCarried, nuint src, nuint n)
    {
        if (!s_anyTable)
            return;

        if (n == 0 || !TryLocateCarried(srcCarried, ref src, out object srcStorage, out nuint srcOffset) ||
            !s_tables.TryGetValue(srcStorage, out Table? from))
        {
            // Nothing to carry from the source, so the destination's words were overwritten by numbers.
            ClearEntries(dstCarried, dst, n);
            return;
        }

        if (!TryLocateCarried(dstCarried, ref dst, out object dstStorage, out nuint dstOffset, evenWithoutTable: true))
            return;

        Table to = TableFor(dstStorage);
        (Table first, Table second) = from.Id <= to.Id ? (from, to) : (to, from);

        lock (first)
        {
            lock (second)
            {
                // Snapshot the source's entries before the destination changes: the two ranges may be one storage.
                List<(nuint delta, nuint number, object referent)> carried = [];
                nuint words = n / (nuint)IntPtr.Size;

                for (nuint i = 0; i < words; i++)
                {
                    nuint delta = i * (nuint)IntPtr.Size;
                    nuint word = ReadWordAt(dst + delta);

                    if (Find(from, srcOffset + delta, word) is { } entry)
                        carried.Add((delta, entry.Number, entry.Referent));
                }

                for (nuint i = 0; i < words; i++)
                    to.Slots.Remove(dstOffset + i * (nuint)IntPtr.Size);

                foreach ((nuint delta, nuint number, object referent) in carried)
                    Add(to, dstOffset + delta, number, referent);
            }
        }
    }

    /// <summary>A clear door: the word slots in the cleared range drop every entry.</summary>
    public static void ClearEntries(object? carried, nuint address, nuint n)
    {
        if (!s_anyTable)
            return;

        if (n == 0 || !TryLocateCarried(carried, ref address, out object storage, out nuint offset) ||
            !s_tables.TryGetValue(storage, out Table? table))
            return;

        lock (table)
        {
            if (table.Slots.Count == 0)
                return;

            nuint end = offset + n;
            List<nuint>? doomed = null;

            foreach (nuint slot in table.Slots.Keys)
            {
                if (slot + (nuint)IntPtr.Size > offset && slot < end)
                    (doomed ??= []).Add(slot);
            }

            if (doomed is not null)
            {
                foreach (nuint slot in doomed)
                    table.Slots.Remove(slot);
            }
        }
    }

    // A copy or clear door's pointer: the box it carries, and the address it names. A word that has entries is in storage
    // a pin holds still (a store takes the pin through its slot's box), but the door's pointer may be another box over
    // that storage that nothing pinned: a FromBox mint, whose number was read under a `fixed` that has ended. Such a box
    // names its source at offset 0, so once its storage is known to have a table it is pinned through its own number
    // conversion, and the address is the one that conversion answers. Storage with no table is never pinned here.
    private static bool TryLocateCarried(object? carried, ref nuint address, out object storage, out nuint offset, bool evenWithoutTable = false)
    {
        storage = null!;
        offset = 0;

        if (NativeDerivation.HeldSource(carried) is not { } held)
            return false;

        if (!held.TryHeldTarget(out object target, out nuint start, out nuint end))
        {
            // The destination of a copy that carries entries is pinned whether or not it has a table yet.
            if (!ReferenceEquals(held, carried) || held is not INilPointer { PinnableStorage: { } candidate } ||
                (!evenWithoutTable && !s_tables.TryGetValue(candidate, out _)) || held is not IGoReflectBox box)
                return false;

            address = box.NativeArgumentAddress();

            if (!held.TryHeldTarget(out target, out start, out end))
                return false;
        }
        else if ((address < start || address >= end) && ReferenceEquals(held, carried) && held is IGoReflectBox box &&
                 ((INilPointer)held).IsPinnedAt(box.NativeArgumentAddress()))
        {
            // A FromBox mint of a box that IS pinned: its carried number is transient, its own number is not.
            address = box.NativeArgumentAddress();
        }

        if (address < start || address >= end)
            return false;

        storage = target;
        offset = address - start;
        return true;
    }

    private static unsafe nuint ReadWordAt(nuint address) => *(nuint*)address;

    private static Entry? Find(Table table, nuint offset, nuint word)
    {
        if (word == 0 || !table.Slots.TryGetValue(offset, out List<Entry>? bag))
            return null;

        foreach (Entry entry in bag)
        {
            if (entry.Number == word)
                return entry;
        }

        return null;
    }

    private static void Add(Table table, nuint offset, nuint word, object? referent)
    {
        if (word == 0 || referent is null)
            return;

        if (!table.Slots.TryGetValue(offset, out List<Entry>? bag))
            table.Slots[offset] = bag = [];

        foreach (Entry entry in bag)
        {
            if (entry.Number == word && ReferenceEquals(entry.Referent, referent))
            {
                entry.Count++;
                return;
            }
        }

        bag.Add(new Entry(word, referent));
    }

    // Removes ONE count of one entry for the displaced word, preferring an entry whose referent is not the one the same
    // operation just stored (design 18.5 finding 1), and drops an entry at count 0 and a bag left empty.
    private static void Remove(Table table, nuint offset, nuint displaced, object? stored)
    {
        if (displaced == 0 || !table.Slots.TryGetValue(offset, out List<Entry>? bag))
            return;

        int chosen = -1;

        for (int i = 0; i < bag.Count; i++)
        {
            if (bag[i].Number != displaced)
                continue;

            chosen = i;

            if (!ReferenceEquals(bag[i].Referent, stored))
                break;
        }

        if (chosen < 0)
            return;

        if (--bag[chosen].Count == 0)
            bag.RemoveAt(chosen);

        if (bag.Count == 0)
            table.Slots.Remove(offset);
    }

    /// <summary>
    /// The post-projection check (design 18.7 finding 1): whether <paramref name="word"/>, the number a store took through
    /// the pinning projection, names <paramref name="referent"/> as storage that stays put while the word is published:
    /// a native alias, the zerobase, an order token, or an address the referent's box (or the Go value it was derived
    /// from) holds still. A pinned address that is also a TAG is refused by name (design decision (B), 2026-11-20).
    /// </summary>
    public static bool NamesHeldStorage(object referent, nuint word)
    {
        bool pinnedAt = referent is INilPointer pinned && pinned.IsPinnedAt(word);

        if (ManagedPointerTokens.IsTaggedToken(word))
        {
            if (pinnedAt)
                throw RuntimeErrorPanic.RuntimeRaised($"unsafe.Pointer store: the number 0x{word:X} is both a pinned address and an order token's tag");

            return true;
        }

        if (word == GoZeroBase.Address || pinnedAt)
            return true;

        if (referent is IGoReflectBox { NativeAddress: not 0 })
            return true;

        return NativeDerivation.HeldSource(referent) is { } held && held.HeldBytesFromExtent(word);
    }

    /// <summary>Whether a pointer's referent is a native box over C memory alone (no source, no pin, no token).</summary>
    public static bool IsCMemoryAlone(object referent) => referent is INativeRooted { IsGenuinelyNativeRoot: true };

    /// <summary>Whether a number names Go memory: an order token, an address the exact registry knows, or one inside a live pin.</summary>
    public static bool NamesGoMemory(nuint word) =>
        word != 0 && (ManagedPointerTokens.IsTaggedToken(word) || ManagedPointerTokens.Resolve(word) is not null || PinExtents.Find(word) is not null);

    /// <summary>TEST SEAM: the number of live entries over a storage object's slots (counts summed).</summary>
    internal static int EntryCount(object storage)
    {
        if (!s_tables.TryGetValue(storage, out Table? table))
            return 0;

        lock (table)
        {
            int count = 0;

            foreach (List<Entry> bag in table.Slots.Values)
            {
                foreach (Entry entry in bag)
                    count += entry.Count;
            }

            return count;
        }
    }
}
