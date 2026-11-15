// PinExtents.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
/// </remarks>
internal sealed class PinExtents
{
    private const int PageShift = 12;

    private static PinExtents? s_current;

    private readonly ConcurrentDictionary<nuint, Page> m_pages = new();

    private PinExtents() { }

    /// <summary>The index, or null in a process that links no cgo package.</summary>
    internal static PinExtents? Current => Volatile.Read(ref s_current);

    /// <summary>
    /// Switches the index on, once per process, and fills it with every pin the exact registry holds a live registrant
    /// for. A pin made before this runs that is not in the exact registry stays out of it: the early-pin residual, row
    /// 11 of the design's section 4.
    /// </summary>
    internal static void Activate()
    {
        if (Volatile.Read(ref s_current) is not null)
            return;

        if (Interlocked.CompareExchange(ref s_current, new PinExtents(), null) is not null)
            return;

        ManagedPointerTokens.VisitPinnedRegistrants(static box =>
        {
            if (box is IHeldPin { HeldPin: { } pin })
                pin.EnterIndex();
        });
    }

    /// <summary>
    /// The live pin whose extent holds <paramref name="address"/>, as a STRONG reference the caller keeps for as long as
    /// it relies on the answer, or null when no pin holds it.
    /// </summary>
    internal static PinnedBuffer? Find(nuint address)
    {
        PinExtents? index = Volatile.Read(ref s_current);

        if (index is null || address == 0 || !index.m_pages.TryGetValue(address >> PageShift, out Page? page))
            return null;

        for (Node? node = page.Head; node is not null; node = node.Next)
        {
            if (node.Dead == 0 && address >= node.Base && address < node.End && node.Owner.TryGetTarget(out PinnedBuffer? owner))
                return owner;
        }

        return null;
    }

    // The extent [start, end) of owner's pinned object, in one node per page it covers. Returns the node, or the
    // nodes' array for an extent over more than one page: what Remove takes back.
    internal object Add(PinnedBuffer owner, nuint start, nuint end)
    {
        WeakReference<PinnedBuffer> weak = new(owner, trackResurrection: false);
        nuint first = start >> PageShift, last = (end - 1) >> PageShift;
        Node[]? many = first == last ? null : new Node[(int)(last - first + 1)];
        Node? one = null;

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

            if (many is null)
                one = node;
            else
                many[(int)(number - first)] = node;
        }

        return (object?)many ?? one!;
    }

    internal void Remove(object extent)
    {
        if (extent is Node node)
        {
            RemoveOne(node.Base >> PageShift, node);
            return;
        }

        Node[] nodes = (Node[])extent;
        nuint first = nodes[0].Base >> PageShift;

        for (int i = 0; i < nodes.Length; i++)
            RemoveOne(first + (nuint)i, nodes[i]);
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
