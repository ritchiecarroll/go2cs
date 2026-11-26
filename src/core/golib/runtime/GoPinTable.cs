// GoPinTable.cs - Gbtc
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
/// The pin table behind <c>runtime.Pinner</c>: Go's pinner bits, keyed by the allocation rather than the span. A pinned
/// allocation is HELD STILL, not only recorded (cgo S6, docs/phase4/DESIGN-cgo-phase3-marshaling.md 21, 21.7, 21.8).
/// </summary>
/// <remarks>
/// <para>
/// WHY IN GOLIB. The cgo doors that must see a pin live here (<see cref="GoCgoArgs"/>), and golib cannot reference
/// <c>runtime</c>. <c>runtime</c>'s hand-owned Pinner calls this table; so there is one table, not two.
/// </para>
/// <para>
/// WHAT A PIN HOLDS. Go's pin holds the whole object. In go2cs an inline array is its own allocation, so pinning a value
/// takes a handle over EVERY allocation inside it: the box's own slot and each backing <see cref="GoCgoImages.ImageKeys"/>
/// finds in its value (21.7, MUST 1). An allocation the CLR refuses to pin (one holding references) is recorded and not
/// held; C is never handed its address. The reverse is not done and cannot be: pinning an element does not pin the struct
/// that holds its array, since an array has no reference back to its containers.
/// </para>
/// <para>
/// WHO OWNS A HANDLE. Each first pin of an allocation takes its OWN handles; the table never counts on a pin another
/// owner holds, since that owner may release it while C still keeps the address (21.7, MUST 2). A repeat pin of the same
/// allocation counts in the table's record and allocates nothing. The last unpin frees the handles.
/// </para>
/// <para>
/// LOCKING. Lookups read a weak-keyed table without a lock. Count transitions and handle allocation take the one lock,
/// where Go takes span.speciallock. Weak keys: the table is never the reason an allocation stays alive; the HOLD is the
/// Pinner's own list.
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GoPinTable
{
    private sealed class PinRecord
    {
        public int Count;
        public PinnedBuffer[] Pins = [];
    }

    private sealed class StorageCount
    {
        public int Count;
    }

    private static readonly ConditionalWeakTable<object, PinRecord> s_records = new();
    private static readonly ConditionalWeakTable<object, StorageCount> s_storages = new();
    private static readonly object s_lock = new();
    private static int s_live;
    private static long s_handlesTaken;

    /// <summary>Whether any allocation is pinned at all: the doors' one-read fast path.</summary>
    internal static bool Any => Volatile.Read(ref s_live) != 0;

    /// <summary>
    /// TEST SEAM (21.7, SHOULD 4): the handles the table has taken since the process started. A B/op row cannot see a
    /// handle, so the poll guard reads this across its I/O.
    /// </summary>
    public static long HandlesTaken => Interlocked.Read(ref s_handlesTaken);

    /// <summary>Pins <paramref name="referent"/>: counts a repeat pin, and holds every allocation of a first one still.</summary>
    public static void Pin(object referent)
    {
        lock (s_lock)
        {
            if (s_records.TryGetValue(referent, out PinRecord? record))
            {
                record.Count++;
                return;
            }

            record = new PinRecord { Count = 1, Pins = HoldAll(referent) };
            s_records.Add(referent, record);
            s_live++;
        }
    }

    /// <summary>Unpins <paramref name="referent"/> once; false when it was not pinned (Go's "object already unpinned").</summary>
    public static bool Unpin(object referent)
    {
        lock (s_lock)
        {
            if (!s_records.TryGetValue(referent, out PinRecord? record))
                return false;

            if (--record.Count > 0)
                return true;

            s_records.Remove(referent);
            s_live--;

            foreach (PinnedBuffer pin in record.Pins)
            {
                if (pin.PinnedTarget is { } storage && s_storages.TryGetValue(storage, out StorageCount? held) && --held.Count == 0)
                    s_storages.Remove(storage);

                pin.Dispose();
            }

            return true;
        }
    }

    /// <summary>Whether <paramref name="referent"/> is pinned.</summary>
    public static bool IsPinned(object referent) => s_records.TryGetValue(referent, out _);

    /// <summary>How many times <paramref name="referent"/> is pinned.</summary>
    public static int Count(object referent) => s_records.TryGetValue(referent, out PinRecord? record) ? record.Count : 0;

    /// <summary>Whether a pin this table owns holds <paramref name="storage"/> (an allocation inside a pinned value).</summary>
    internal static bool HoldsStorage(object storage) => s_storages.TryGetValue(storage, out _);

    // Every allocation of the referent that the CLR will hold still: a box's slot and the backings inside its value, or an
    // array and the backings inside its elements. Each gets its own handle, counted in s_storages so a door asking about a
    // backing (an element pointer into an inline array) sees the pin of the value that contains it.
    private static PinnedBuffer[] HoldAll(object referent)
    { if (referent is not null) return []; // PROBE ONLY: real pins off (S6 SHOULD 4); never lands
        // The common case, one allocation: a box whose value holds no reference cannot hold an inline array, so its slot
        // is the whole value, and neither the value is read (a boxing copy) nor a list built.
        if (referent is INilPointer { PinnableStorage: Array onlySlot } && onlySlot.GetType().GetElementType() is { } onlyElement && !NativeStructMarshal.ContainsReferences(onlyElement))
            return PinnedBuffer.PinOnly(onlySlot) is { } only ? [Held(onlySlot, only)] : [];

        List<object> storages = [];

        switch (referent)
        {
            case Array array:
                storages.Add(array);

                // An element type holding no reference holds no inline array (an array<T> is a reference here), so the walk,
                // which boxes every element, is skipped (P3's SHOULD 5 on 2ce6812305).
                if (array.GetType().GetElementType() is { IsValueType: true, IsPrimitive: false, IsEnum: false } elementType && NativeStructMarshal.ContainsReferences(elementType))
                {
                    foreach (object? element in array)
                        AddBackings(storages, element);
                }

                break;

            case INilPointer box:
                if (box.PinnableStorage is { } slot)
                    storages.Add(slot);

                if (referent is IGoReflectBox reflect && !box.IsNilPointer)
                    AddBackings(storages, reflect.ReadBoxSlot());

                break;
        }

        List<PinnedBuffer> pins = [];

        foreach (object storage in storages)
        {
            if (PinnedBuffer.PinOnly(storage) is { } pin)
                pins.Add(Held(storage, pin));
        }

        return [.. pins];
    }

    // Counts a handle the table took over storage, so a door asking about that storage sees it held.
    private static PinnedBuffer Held(object storage, PinnedBuffer pin)
    {
        Interlocked.Increment(ref s_handlesTaken);

        if (s_storages.TryGetValue(storage, out StorageCount? held))
            held.Count++;
        else
            s_storages.Add(storage, new StorageCount { Count = 1 });

        return pin;
    }

    private static void AddBackings(List<object> storages, object? value)
    {
        if (value is null || value.GetType() is not { IsValueType: true, IsPrimitive: false, IsEnum: false })
            return;

        object[] keys = GoCgoImages.ImageKeys(value, value);

        for (int i = 1; i < keys.Length; i++)
            storages.Add(keys[i]);
    }
}
