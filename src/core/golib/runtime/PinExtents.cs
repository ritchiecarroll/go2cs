// PinExtents.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace go.golib;

/// <summary>
/// Every extent of managed memory golib holds pinned, by 4 KiB page, so the cgo void pointer's door can tell a number
/// INSIDE pinned Go memory from a number in C memory (docs/phase4/DESIGN-cgo-pin-extents.md, D1 to D4). The exact
/// registry (<see cref="ManagedPointerTokens"/>) answers only for the exact numbers it was handed; this answers for
/// every byte of every pin, end-exclusive, as Go's own <c>cgoIsGoPointer</c> answers through its span lookup.
/// </summary>
/// <remarks>
/// <para>
/// Only a process that links a cgo package has one: <see cref="GoCgoLibraries"/>' registration, which every <c>-cgo</c>
/// package's module initializer runs, switches it on, and fills it with the pins the exact registry already holds. In
/// every other process <see cref="Current"/> stays null and a pin pays one read of it.
/// </para>
/// <para>
/// The reader takes no lock: a page's extents are a pushed list, and a removed extent is marked dead before any writer
/// unlinks it, so a reader standing on a node still reaches the live rest of the list. A writer locks only the page it
/// changes, with a .NET lock and never a Go runtime lock, because removal also runs on the finalizer thread (the
/// design's section 6, lane G's answer to QG-1).
/// </para>
/// <para>
/// Each extent holds its <see cref="PinnedBuffer"/> through a SHORT weak reference: the index must not root a buffer
/// that leaves it from its own finalizer, and a short weak target is cleared before that finalizer runs, so a reader
/// can never take a buffer whose pin is about to be released. The handle lives inside a <see cref="WeakReference{T}"/>
/// rather than as a bare <c>GCHandle</c> freed at removal: a lock-free reader may still be standing on a removed node,
/// and a handle freed under it could be reissued to another object. The weak reference's own finalizer frees its handle
/// once no reader can reach the node.
/// </para>
/// <para>
/// An object over more than 2 pages is not in the pages at all: it is one entry in a side list, which the door asks only
/// for a number inside the envelope of the live large entries, and every further pin of the same object joins that entry,
/// found by its base. So a pin costs the index the same whatever the size of the object it pins, however many pins of
/// that object are live, and however many such objects are pinned (P3's review of the seat, item 1, and its re-read,
/// items 2 and 3).
/// </para>
/// </remarks>
internal sealed class PinExtents
{
    private const int PageShift = 12;

    // An extent over MORE than this many pages is one entry in the large list, never a node per page: a pin's cost to the
    // index must not grow with the object it pins (P3's review, item 1: a 16 MiB pin cost 4,097 nodes, 0.6 ms and 623 KB;
    // its re-read, item 2: 1,000 live pins of one 60 KiB buffer held 16,000 nodes, 1.6-2.6 us and 1,089 B a pin). A page
    // costs about 120 ns and 68-152 B a pin, a large entry 112-136 B and 200-380 ns whatever the size, so the pages win
    // only for an object over one or two of them (a 4 KiB array's data spans two).
    private const int LargePages = 2;

    private static PinExtents? s_current;

    private readonly ConcurrentDictionary<nuint, Page> m_pages = new();

    // The large list: a pushed list read without a lock, written under m_largeGate, with each entry also found by its
    // base (m_largeByBase, writer-only) so a pin joins or adds in O(1) however many objects are pinned. [m_low, m_high)
    // covers every live entry: widened BEFORE an entry is linked, and reset only when the list empties -- narrowing it on
    // every release is a walk of the list (P3's re-read, item 3: at 1,000 entries a release cost 2.7-3.0 us), so between
    // those points it may be wider than what is live, which costs a walk and never a wrong answer. A number outside it never
    // walks the list. C memory is outside it under the GC's regions (the 64-bit default since .NET 7), which reserve the
    // managed heap as one range apart from native allocations. Under segments (a 32-bit process, or a configured segments
    // GC), large objects and native memory can interleave, and a C number inside the envelope walks the list: slower, and
    // never a wrong answer. A number inside it that no entry holds is a Go-memory number the door goes on to refuse, or
    // unpinned managed memory: the walk sits on those paths only, and costs about 3 ns an entry.
    private readonly object m_largeGate = new();
    private readonly Dictionary<nuint, Large> m_largeByBase = new();
    private volatile Large? m_large;
    private volatile nuint m_low = nuint.MaxValue;
    private volatile nuint m_high;

    // Set once the activating thread's backfill is done (P3's review, item 2): until then the index lacks the early pins,
    // and a miss is the unsafe direction, so a second activation and the door wait for it.
    private volatile bool m_ready;
    private readonly int m_activatingThread = Environment.CurrentManagedThreadId;

    private PinExtents() { }

    /// <summary>The index, or null in a process that links no cgo package.</summary>
    internal static PinExtents? Current => Volatile.Read(ref s_current);

    /// <summary>
    /// Switches the index on, once per process, and fills it with every pin the exact registry holds a live registrant
    /// for. A pin made before this runs that is not in the exact registry stays out of it: the early-pin residual, row
    /// 11 of the design's section 4. That includes a NATIVE box's pin (a reinterpret's, say): the backfill reads a box's
    /// pin only where <c>IsPinnedAt</c> answers, and it answers false for every box with a native address.
    /// </summary>
    internal static void Activate()
    {
        PinExtents? index = Volatile.Read(ref s_current);

        if (index is null)
        {
            PinExtents created = new();
            index = Interlocked.CompareExchange(ref s_current, created, null);

            if (index is null)
            {
                try
                {
                    ManagedPointerTokens.VisitPinnedRegistrants(static box =>
                    {
                        if (box is IHeldPin { HeldPin: { } pin })
                            pin.EnterIndex();
                    });
                }
                finally
                {
                    created.m_ready = true;
                }

                return;
            }
        }

        index.WaitReady();
    }

    private void WaitReady()
    {
        if (m_ready || m_activatingThread == Environment.CurrentManagedThreadId)
            return;

        SpinWait spin = default;

        while (!m_ready)
            spin.SpinOnce();
    }

    /// <summary>
    /// The live pin whose extent holds <paramref name="address"/>, as a STRONG reference the caller keeps for as long as
    /// it relies on the answer, or null when no pin holds it.
    /// </summary>
    /// <remarks>
    /// The strong reference holds the buffer, and so its pin, against its FINALIZER only, not against an explicit
    /// Dispose: whoever owns the buffer can still release it while a caller holds this answer. The door only refuses on
    /// it, which needs nothing held. A caller that must keep the bytes still through a native call (3.2) takes a pin of
    /// its own, scoped to the call, rather than relying on this one.
    /// </remarks>
    internal static PinnedBuffer? Find(nuint address)
    {
        PinExtents? index = Volatile.Read(ref s_current);

        if (index is null || address == 0)
            return null;

        if (!index.m_ready)
            index.WaitReady();

        if (index.m_pages.TryGetValue(address >> PageShift, out Page? page))
        {
            for (Node? node = page.Head; node is not null; node = node.Next)
            {
                if (node.Dead == 0 && address >= node.Base && address < node.End && node.Owner.TryGetTarget(out PinnedBuffer? owner))
                    return owner;
            }
        }

        // A page holding small pins can also hold a large object's first or last bytes, so the large list is asked on any
        // page miss -- inside the envelope only.
        return address >= index.m_low && address < index.m_high ? index.FindLarge(address) : null;
    }

    // The backfill's own entries: buffers made before the index switched on carry no entry field (a process with no cgo
    // package must allocate the same buffer it did before the index existed), so their entries are kept here, keyed by
    // the buffer's pinned handle. A handle is unique while allocated, and the entry leaves before the handle is freed.
    // Keyed by handle rather than by buffer: the release runs from the buffer's own finalizer, when a weak key would
    // already be gone.
    private readonly Dictionary<nint, object> m_backfilled = new();
    private readonly object m_backfillGate = new();

    internal void EnterBackfilled(PinnedBuffer buffer)
    {
        lock (m_backfillGate)
        {
            // The claim first: a buffer whose release has begun is left out, and one that is claimed is not released
            // until the claim ends, so the handle read below is never of a freed handle.
            if (!buffer.TryBeginBackfill())
                return;

            try
            {
                if (m_backfilled.ContainsKey(buffer.HandleKey) || !buffer.TryExtent(out nuint start, out nuint end))
                    return;

                m_backfilled.Add(buffer.HandleKey, Add(buffer, start, end));
            }
            finally
            {
                buffer.EndBackfill();
            }
        }
    }

    // Called by a release after it has closed the buffer, so no backfill enters it from here on.
    internal void LeaveBackfilled(PinnedBuffer buffer)
    {
        object? extent;

        lock (m_backfillGate)
        {
            if (!m_backfilled.Remove(buffer.HandleKey, out extent))
                return;
        }

        Remove(extent);
    }

    private PinnedBuffer? FindLarge(nuint address)
    {
        for (Large? entry = m_large; entry is not null; entry = entry.Next)
        {
            if (entry.Dead == 0 && address >= entry.Base && address < entry.End && entry.LiveOwner() is { } owner)
                return owner;
        }

        return null;
    }

    /// <summary>
    /// TEST DIAGNOSTIC: how many live index entries describe some byte of [<paramref name="start"/>,
    /// <paramref name="end"/>). What a pin costs the index is the arm that reads it.
    /// </summary>
    /// <remarks>
    /// A pin is one entry however many pages its bytes cover. Add gives it a node per page, each carrying the whole extent,
    /// so a node is counted only on the first page of the window it covers: counting every node read a pin that crosses a
    /// page boundary once per page (GoCgoArgsTests.NamedArrayPin.cs read 2 whenever the allocator put its 64-byte backing
    /// across a page). Only tests call this; Add, Find and Remove are unchanged.
    /// </remarks>
    internal static int EntriesOver(nuint start, nuint end)
    {
        PinExtents? index = Volatile.Read(ref s_current);
        int count = 0;

        if (index is null || end <= start)
            return 0;

        for (nuint number = start >> PageShift; number <= (end - 1) >> PageShift; number++)
        {
            if (!index.m_pages.TryGetValue(number, out Page? page))
                continue;

            for (Node? node = page.Head; node is not null; node = node.Next)
            {
                if (node.Dead == 0 && node.Base < end && node.End > start && Math.Max(node.Base, start) >> PageShift == number)
                    count++;
            }
        }

        return count + index.LargeOver(start, end, countHolders: true);
    }

    /// <summary>TEST DIAGNOSTIC: how many live large-list entries -- distinct large OBJECTS -- overlap [start, end).</summary>
    internal static int LargeEntriesOver(nuint start, nuint end) =>
        Volatile.Read(ref s_current)?.LargeOver(start, end, countHolders: false) ?? 0;

    private int LargeOver(nuint start, nuint end, bool countHolders)
    {
        int count = 0;

        for (Large? entry = m_large; entry is not null; entry = entry.Next)
        {
            if (entry.Dead != 0 || entry.Base >= end || entry.End <= start)
                continue;

            count++;

            if (!countHolders)
                continue;

            for (Holder? holder = entry.Holders; holder is not null; holder = holder.Next)
            {
                if (holder.Dead == 0)
                    count++;
            }
        }

        return count;
    }

    // The extent [start, end) of owner's pinned object, in one node per page it covers, chained page to page through
    // NextPage. Returns the first node: what Remove takes back.
    //
    // NextPage is WRITER-ONLY state. It is written here, before Add returns, so before the caller keeps the node (the
    // indexed buffer's own field, set in its constructor, or the backfill's table, under its gate), and it is read only by
    // Remove, which starts from that kept node. No reader follows it: Find walks one page's list through Next and nothing else,
    // and Remove never changes NextPage. So a reader cannot meet a node whose next-page link is being written or
    // unlinked; it meets only Next, which RemoveOne changes under the page lock exactly as before.
    //
    // An extent over more than LargePages pages takes the large list instead (AddLarge), and answers its entry or, when it
    // joins a live entry for the same object, its holder.
    internal object Add(PinnedBuffer owner, nuint start, nuint end)
    {
        WeakReference<PinnedBuffer> weak = new(owner, trackResurrection: false);
        nuint first = start >> PageShift, last = (end - 1) >> PageShift;

        if (last - first >= LargePages)
            return AddLarge(weak, start, end);

        Node? head = null, previous = null;

        for (nuint number = first; number <= last; number++)
        {
            Node node = new(start, end, weak);

            while (true)
            {
                Page page = m_pages.GetOrAdd(number, static _ => new Page());

                lock (page)
                {
                    // A page emptied and dropped while this writer waited: take the dictionary's current one.
                    if (page.Dropped)
                        continue;

                    node.Next = page.Head;
                    page.Head = node;
                    page.Live++;
                    break;
                }
            }

            if (previous is null)
                head = node;
            else
                previous.NextPage = node;

            previous = node;
        }

        return head!;
    }

    internal void Remove(object extent)
    {
        switch (extent)
        {
            case Node head:
                nuint number = head.Base >> PageShift;

                for (Node? node = head; node is not null; node = node.NextPage, number++)
                    RemoveOne(number, node);

                break;

            case Large entry:
                // Gone first, so a reader from here on asks the holders and not this owner.
                entry.OwnerGone = 1;
                LeaveLarge(entry, leavingHolder: false);
                break;

            case Holder holder:
                holder.Dead = 1;
                LeaveLarge(holder.Of, leavingHolder: true);
                break;
        }
    }

    // One entry per large OBJECT. Golib's box pins are released by finalizer, so a loop over one big buffer holds many
    // pins of it at once: each later pin JOINS the live entry with the same extent as a holder, so the list holds one entry
    // per distinct object, and a pin costs one lock and one lookup by base however many pins and objects are live.
    private object AddLarge(WeakReference<PinnedBuffer> weak, nuint start, nuint end)
    {
        lock (m_largeGate)
        {
            // Every entry in the table is linked and live (Dead is set only as it leaves, under this lock), so its count is
            // above 0. Two live pinned objects cannot share a base, so a base names one object.
            if (m_largeByBase.TryGetValue(start, out Large? live))
            {
                Debug.Assert(live.End == end, "one pinned object, two extents");

                Holder holder = new(weak, live) { Next = live.Holders };
                live.Holders = holder;
                live.Count++;
                return holder;
            }

            Large created = new(start, end, weak);

            // Widen before linking: a reader that can see the entry sees an envelope that covers it.
            if (start < m_low)
                m_low = start;

            if (end > m_high)
                m_high = end;

            Large? head = m_large;
            created.Next = head;

            if (head is not null)
                head.Previous = created;

            m_large = created;
            m_largeByBase.Add(start, created);
            return created;
        }
    }

    private void LeaveLarge(Large entry, bool leavingHolder)
    {
        lock (m_largeGate)
        {
            if (leavingHolder)
                entry.DeadHolders++;

            if (--entry.Count > 0)
            {
                if (entry.DeadHolders > 8 && entry.DeadHolders > entry.Count)
                    CompactHolders(entry);

                return;
            }

            // The last pin of the object: dead, out of the table, unlinked in O(1) through Previous. Every linked entry is
            // live, so the predecessor whose Next is rewritten here is live, and a reader standing on this entry keeps its
            // own Next, which is never rewritten.
            entry.Dead = 1;
            m_largeByBase.Remove(entry.Base);

            Large? previous = entry.Previous, next = entry.Next;

            if (previous is null)
                m_large = next;
            else
                previous.Next = next;

            if (next is not null)
                next.Previous = previous;

            // The envelope resets only when nothing is left (see m_low): narrowing it here would walk the list.
            if (m_large is null)
            {
                m_low = nuint.MaxValue;
                m_high = 0;
            }
        }
    }

    // Unlinks dead holders in place, by the page list's rule: only a live holder's Next is rewritten, so a reader standing
    // on a dead one still reaches the live rest.
    private static void CompactHolders(Large entry)
    {
        Holder? head = entry.Holders;

        while (head is not null && head.Dead != 0)
            head = head.Next;

        entry.Holders = head;

        for (Holder? current = head; current is not null;)
        {
            Holder? next = current.Next;

            while (next is not null && next.Dead != 0)
                next = next.Next;

            current.Next = next;
            current = next;
        }

        entry.DeadHolders = 0;
    }

    private void RemoveOne(nuint number, Node node)
    {
        // Dead first, so a reader that reaches the node from here on passes it by.
        node.Dead = 1;

        if (!m_pages.TryGetValue(number, out Page? page))
            return;

        lock (page)
        {
            page.Live--;
            page.DeadCount++;

            if (page.Live == 0)
            {
                page.Dropped = true;
                page.Head = null;
                m_pages.TryRemove(new KeyValuePair<nuint, Page>(number, page));
                return;
            }

            if (page.DeadCount <= 8 || page.DeadCount * 2 <= page.Live + page.DeadCount)
                return;

            // Unlink the dead in place. A reader standing on a dead node keeps that node's own Next, so it still
            // reaches a live suffix of the list.
            Node? head = page.Head;

            while (head is not null && head.Dead != 0)
                head = head.Next;

            page.Head = head;

            for (Node? current = head; current is not null;)
            {
                Node? next = current.Next;

                while (next is not null && next.Dead != 0)
                    next = next.Next;

                current.Next = next;
                current = next;
            }

            page.DeadCount = 0;
        }
    }

    /// <summary>The byte length of a pinned object's data, end-exclusive from its pinned address.</summary>
    internal static nuint ExtentOf(object target, nint length)
    {
        switch (target)
        {
            case byte[] bytes:
                return (nuint)bytes.Length;

            case string text:
                return (nuint)text.Length * sizeof(char);

            case Array array:
                return (nuint)array.LongLength * (nuint)ElementSize(array.GetType());

            default:
                // Unreachable today (P3's review, item 5): every site that pins hands GCHandle an array -- a value slot,
                // a backing array, IArray.Source, a byte[] -- or a string's re-pin, and a boxed @string holds a byte[],
                // so GCHandle refuses it before it gets here. An object that is neither would leave its interior numbers
                // past byte 0 outside the index.
                Debug.Fail($"a pinned {target.GetType()} is neither an array nor a string: its extent is a guess");
                return (nuint)Math.Max(length, 1);
        }
    }

    private static readonly ConcurrentDictionary<Type, int> s_elementSizes = new();

    private static int ElementSize(Type arrayType) =>
        s_elementSizes.GetOrAdd(arrayType, static type => RuntimeHelpers.SizeOf(type.GetElementType()!.TypeHandle));

    private sealed class Node(nuint start, nuint end, WeakReference<PinnedBuffer> owner)
    {
        internal readonly nuint Base = start;
        internal readonly nuint End = end;
        internal readonly WeakReference<PinnedBuffer> Owner = owner;
        internal volatile int Dead;
        internal volatile Node? Next;

        // The same pin's node on the next page (writer-only; see Add).
        internal Node? NextPage;
    }

    // A large object's one entry: its first pin is the owner, every later pin a holder. Count is every live pin of it and
    // changes only under m_largeGate; the rest is read by Find without a lock.
    private sealed class Large(nuint start, nuint end, WeakReference<PinnedBuffer> owner)
    {
        internal readonly nuint Base = start;
        internal readonly nuint End = end;
        internal readonly WeakReference<PinnedBuffer> Owner = owner;
        internal volatile int OwnerGone;
        internal volatile Holder? Holders;
        internal volatile int Dead;
        internal volatile Large? Next;

        // Writer-only, under the list's lock: the unlink's O(1) predecessor. No reader follows it.
        internal Large? Previous;
        internal int Count = 1;
        internal int DeadHolders;

        // The first pin of the object still alive, as a strong reference.
        internal PinnedBuffer? LiveOwner()
        {
            if (OwnerGone == 0 && Owner.TryGetTarget(out PinnedBuffer? owner))
                return owner;

            for (Holder? holder = Holders; holder is not null; holder = holder.Next)
            {
                if (holder.Dead == 0 && holder.Owner.TryGetTarget(out owner))
                    return owner;
            }

            return null;
        }
    }

    private sealed class Holder(WeakReference<PinnedBuffer> owner, Large of)
    {
        internal readonly WeakReference<PinnedBuffer> Owner = owner;
        internal readonly Large Of = of;
        internal volatile int Dead;
        internal volatile Holder? Next;
    }

    private sealed class Page
    {
        internal volatile Node? Head;
        internal int Live;
        internal int DeadCount;
        internal bool Dropped;
    }
}

/// <summary>A box's own pin, for the index's backfill at activation.</summary>
internal interface IHeldPin
{
    PinnedBuffer? HeldPin { get; }
}
