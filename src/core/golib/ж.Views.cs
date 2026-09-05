// ж.Views.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.
// ReSharper disable InconsistentNaming

// The FIELD-VIEW CACHE (docs/phase4/DESIGN-field-view-cache.md; COORD ruling f1083bff5, 2026-09-05).
//
// Every `recv.field.Method()` whose callee takes a ж<FieldT> receiver is emitted as
// `Ꮡrecv.of(T.Ꮡfield).Method()`, and `of()` used to mint a FieldRefBox on EVERY call: 64 B and one
// counted object per call, plus the accessor-wrapper weak-table lookup the typed overload paid. The
// sizing (mailbox fb5e64a45) censused 965 receiver-base and 218 parameter-base call sites in 52
// packages paying that box after the ref-primary machinery had freed 1,023 of the 2,036 shape sites.
//
// Now `of()` returns ONE cached view per (box, accessor token), minted on the first call and reused
// for the box's life. Identity is preserved by construction and made stronger: FieldRefBox equality
// already IS (source, token) — the contract the address-keyed semaphores in the hand-owned
// sync/internal-poll implementations rely on — and two calls now return the SAME object rather than
// two equal ones.
//
// Where the cache lives — the TYPE GATE (the spike's arm 3, measured 20.2 ns per call against 33.8
// with the per-call allocation gone; the slot-on-every-box arm read 18.0 at +8 B on every box, the
// weak-table-only arm 37.0):
//   - a SlottedStandardBox<T> carries one ViewEntry? slot (+8 B). Ꮡ<T>() and @new<T>() mint it
//     instead of a StandardBox<T> when BoxShape<T>.Slotted is set — by a [GoBoxViews] attribute on T
//     at type init, or flipped lazily by the FIRST of() on any ж<T>;
//   - a FieldRefBox<T> carries the slot too, so a CHAIN's inner hop (`Ꮡc.of(Conn.Ꮡin).of(halfConn.ᏑMutex)`)
//     caches on the outer view at the slot's cost — a view exists only because an of() happened, so
//     this is 8 B per distinct (box, field) pair, not per box;
//   - every other box — a StandardBox<T> minted before its type flipped, an ElemRefBox, a NativeBox —
//     falls back to a per-T ConditionalWeakTable keyed on the box: the same list, one weak lookup.
//
// The lazy flip's NON-DETERMINISM, named: correctness is identical on both paths (the same view
// object comes back), only the cost placement varies across a process's early life — a box minted
// before its type's first of() pays the weak-table lookup per call for its lifetime. The
// converter-emitted attribute is the deterministic form and is built only if a row shows the early
// boxes mattering (the design, §3).
//
// The nil box never caches: it is a shared static per T and carries no field to alias. Retention is
// the cost that is not per call: a parent keeps one view per distinct field asked of it, for its
// lifetime (the parent → view → parent cycle collects together).

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace go;

/// <summary>
/// One cached field view: the accessor token it was minted for, the view, and the next entry.
/// Nodes are immutable; a new head is published by compare-exchange, so readers never lock.
/// </summary>
internal sealed class ViewEntry
{
    internal readonly Delegate Token;
    internal readonly object View;
    internal readonly ViewEntry? Next;

    internal ViewEntry(Delegate token, object view, ViewEntry? next)
    {
        Token = token;
        View = view;
        Next = next;
    }
}

/// <summary>The weak-table value: a mutable head for a box that has no slot of its own.</summary>
internal sealed class ViewTable
{
    internal ViewEntry? Head;
}

/// <summary>
/// Marks a struct type whose boxes carry the field-view slot from their first mint (the
/// deterministic form of the gate). Without it the type flips at the first <c>of()</c> on any box of
/// it, and boxes minted before that use the weak-table fallback.
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false)]
public sealed class GoBoxViewsAttribute : Attribute
{
}

/// <summary>Per-T shape decision: does a fresh standard box of T carry the view slot?</summary>
public static class BoxShape<T>
{
    // volatile: the lazy flip races benignly with minting; a box minted before the flip simply
    // has no slot and uses the weak table.
    public static volatile bool Slotted = typeof(T).IsDefined(typeof(GoBoxViewsAttribute), inherit: false);
}

/// <summary>A standard box that carries the view slot (a consumer type's box).</summary>
public sealed class SlottedStandardBox<T> : StandardBox<T>
{
    internal ViewEntry? m_views;

    public SlottedStandardBox(in T value) : base(value)
    {
    }
}

/// <summary>
/// The cache's one switch, for the guard's negative arm only: with it set, <c>of()</c> mints a fresh
/// view on every call exactly as it did before the cache, so the identity assertions must go RED.
/// Never set outside a test.
/// </summary>
public static class FieldViewCache
{
    public static volatile bool Disabled;
}

public abstract partial class ж<T>
{
    private static readonly ConditionalWeakTable<ж<T>, ViewTable> s_views = new();

    // Where this box's view list lives: a slot on the two kinds that carry one, the weak table
    // for every other kind.
    private ref ViewEntry? viewsHead()
    {
        if (this is SlottedStandardBox<T> slotted)
            return ref slotted.m_views;

        if (this is FieldRefBox<T> view)
            return ref view.m_views;

        return ref s_views.GetOrCreateValue(this).Head;
    }

    /// <summary>
    /// The untyped overload's cached view: minted through <paramref name="fieldRefFunc"/> on the first
    /// request and reused afterwards. The nil box never caches.
    /// </summary>
    private ж<TElem> viewOf<TElem>(FieldRefFunc<TElem> fieldRefFunc)
    {
        if (m_isNull || FieldViewCache.Disabled)
            return new FieldRefBox<TElem>(this, fieldRefFunc);

        if (tryFindView(fieldRefFunc, out ж<TElem>? found))
            return found;

        return publishView(fieldRefFunc, new FieldRefBox<TElem>(this, fieldRefFunc));
    }

    /// <summary>
    /// The typed overload's cached view. The accessor-wrapper lookup (a weak table keyed on the
    /// accessor) runs only on a MISS, so a hit pays neither the allocation nor that lookup.
    /// </summary>
    private ж<TElem> viewOf<TElem>(FieldRefFunc<T, TElem> fieldRefFunc)
    {
        if (m_isNull || FieldViewCache.Disabled)
            return new FieldRefBox<TElem>(this, FieldRefWrappers<TElem>.For(fieldRefFunc), fieldRefFunc);

        if (tryFindView(fieldRefFunc, out ж<TElem>? found))
            return found;

        return publishView(fieldRefFunc, new FieldRefBox<TElem>(this, FieldRefWrappers<TElem>.For(fieldRefFunc), fieldRefFunc));
    }

    private bool tryFindView<TElem>(Delegate token, out ж<TElem>? view)
    {
        // the lazy flip: the first field view asked of any ж<T> marks T a consumer for every box
        // minted afterwards
        if (!BoxShape<T>.Slotted)
            BoxShape<T>.Slotted = true;

        for (ViewEntry? e = viewsHead(); e is not null; e = e.Next)
        {
            if (ReferenceEquals(e.Token, token) || e.Token.Equals(token))
            {
                view = (ж<TElem>)e.View;
                return true;
            }
        }

        view = null;
        return false;
    }

    private ж<TElem> publishView<TElem>(Delegate token, FieldRefBox<TElem> view)
    {
        ref ViewEntry? head = ref viewsHead();

        while (true)
        {
            // Read the head ONCE, scan that exact list for a racer's entry with this token, and only
            // then compare-exchange against that same head: nothing can be published between the scan
            // and the CAS without the CAS failing. (The first form scanned only AFTER a failed CAS — a
            // caller whose miss preceded a racer's publish, and whose head read followed it, then
            // CAS-succeeded on top of the racer's entry and two views of one field existed. The guard's
            // concurrency arm read it on its first run, in both configurations.)
            ViewEntry? seen = head;

            for (ViewEntry? e = seen; e is not null; e = e.Next)
            {
                if (ReferenceEquals(e.Token, token) || e.Token.Equals(token))
                    return (ж<TElem>)e.View;
            }

            ViewEntry candidate = new(token, view, seen);

            if (ReferenceEquals(Interlocked.CompareExchange(ref head, candidate, seen), seen))
                return view;
        }
    }
}
