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
using System.Diagnostics.CodeAnalysis;
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

    /// <summary>
    /// For the guard only: with it set, the accessor match takes the Native AOT form (Delegate.Equals) under the
    /// JIT too, so the identity assertions run that form where tests run. Never set outside a test.
    /// </summary>
    public static volatile bool CompareByDelegateEquals;
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

        if (this is ElemRefBox<T> element)
            return ref element.m_views;

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
    /// The typed overload's cached view: the HIT path, inlined at every site. Every <c>p.Ꮡf</c> site passes the field's
    /// one token (parity seat #32), so a hit is a reference match on the box's own view slot, sameAccessor's first
    /// clause. Anything else is <see cref="viewOfSlow{TElem}"/>'s, unchanged: a miss, a box whose views live in the weak
    /// table, the nil box, the cache disabled, and an older site's own delegate for the field.
    /// </summary>
    /// <remarks>
    /// It is its own small method so that the JIT inlines it without a profile. Dynamic PGO gives none to a method of
    /// one basic block, and a run of <c>&amp;p.f</c> operands is one: PerfFieldView's <c>step</c> under #32 reached tier 1
    /// with no PGO data and called the whole lookup out of line for every operand (+4.7%, JitDisasm on R-LAPTOP,
    /// 2026-10-30), where its base had inlined it from the profile its delegate-cache branches gave it. Forcing the
    /// whole lookup inline instead read +8.9%. Seat #46; ViewCacheHitPathTests guards the shape.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ж<TElem> viewOf<TElem>(FieldRefFunc<T, TElem> fieldRefFunc)
    {
        if (!FieldViewCache.Disabled)
        {
            ViewEntry? e = this is SlottedStandardBox<T> slotted ? slotted.m_views :
                this is ElemRefBox<T> element ? element.m_views :
                this is FieldRefBox<T> view ? view.m_views : null;

            for (; e is not null; e = e.Next)
            {
                if (ReferenceEquals(e.Token, fieldRefFunc))
                    return (ж<TElem>)e.View;
            }
        }

        return viewOfSlow(fieldRefFunc);
    }

    /// <summary>
    /// The typed overload's full lookup. The accessor-wrapper lookup (a weak table keyed on the
    /// accessor) runs only on a MISS, so a hit pays neither the allocation nor that lookup.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private ж<TElem> viewOfSlow<TElem>(FieldRefFunc<T, TElem> fieldRefFunc)
    {
        if (m_isNull || FieldViewCache.Disabled)
            return new FieldRefBox<TElem>(this, FieldRefWrappers<TElem>.For(fieldRefFunc), fieldRefFunc);

        if (tryFindView(fieldRefFunc, out ж<TElem>? found))
            return found;

        return publishView(fieldRefFunc, new FieldRefBox<TElem>(this, FieldRefWrappers<TElem>.For(fieldRefFunc), fieldRefFunc));
    }

    private bool tryFindView<TElem>(Delegate token, [NotNullWhen(true)] out ж<TElem>? view)
    {
        // the lazy flip: the first field view asked of any ж<T> marks T a consumer for every box
        // minted afterwards
        if (!BoxShape<T>.Slotted)
            BoxShape<T>.Slotted = true;

        for (ViewEntry? e = viewsHead(); e is not null; e = e.Next)
        {
            if (sameAccessor(e.Token, token))
            {
                view = (ж<TElem>)e.View;
                return true;
            }
        }

        view = null;
        return false;
    }

    // Delegate.Equals, answered without it. Every `&x.f` site converts the accessor method group to its
    // OWN cached delegate, so one field reaches a box's list as several instances, and a lookup compares
    // its token against every entry ahead of its own. Delegate.Equals settles two distinct instances in
    // the runtime's method-handle comparison, which made the walk most of the run time of crypto/x509's
    // TestBetterTLS (P-256 field arithmetic, three views a point). For a single-cast delegate the same
    // delegate type, target and method is Delegate.Equals' own answer; Method is cached on the delegate
    // after its first read, and its handle names the one runtime method, as Delegate.Equals compares. A
    // multicast delegate's Target and Method name only its LAST entry, so one still asks Delegate.Equals.
    //
    // Under Native AOT the costs reverse. Delegate.Method is a reflection lookup there, about 0.9 us per read and
    // not cached, while Delegate.Equals compares function pointers in about 12 ns: with the Method form,
    // PerfFieldView read 95 s under AOT against 4.8 s with Delegate.Equals (R-LAPTOP, 2026-10-28; the JIT read
    // 3.4 s against 7.8 s the other way round). IsDynamicCodeSupported is a constant to both compilers, so each
    // takes its own form and the other folds away. run-fieldview-aot-bound.ps1 is the gate.
    private static bool sameAccessor(Delegate entry, Delegate token) =>
        ReferenceEquals(entry, token) ||
        (RuntimeFeature.IsDynamicCodeSupported && !FieldViewCache.CompareByDelegateEquals && entry.HasSingleTarget && token.HasSingleTarget
            ? ReferenceEquals(entry.Target, token.Target) && entry.Method.MethodHandle.Equals(token.Method.MethodHandle) && entry.GetType() == token.GetType()
            : entry.Equals(token));

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
                if (sameAccessor(e.Token, token))
                    return (ж<TElem>)e.View;
            }

            ViewEntry candidate = new(token, view, seen);

            if (ReferenceEquals(Interlocked.CompareExchange(ref head, candidate, seen), seen))
                return view;
        }
    }
}
