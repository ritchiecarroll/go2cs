// ж.Views.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.
// ReSharper disable InconsistentNaming

// ⚠ SPIKE — the seg-3 field-view cache, THREE arms behind compile symbols, NOT a banked design.
// COORD ruling b7e37ab7c (2026-09-04): mechanism B for the `Ꮡx.of(T.Ꮡfield).Method()` shape — the
// FieldRefBox that today is minted on EVERY call (64 B, one counted object, plus the accessor-wrapper
// weak-table lookup the typed of() overload pays) is instead minted ONCE per (box, accessor) and
// reused for the box's lifetime. Identity is preserved by construction: FieldRefBox equality is
// (source, accessor token), and a cached view IS the one object per (source, token), which is the
// stronger form of what the address-keyed semaphores in the hand-owned sync/internal-poll
// implementations depend on.
//
//   VIEWS_SLOT   arm 1 — one reference slot on ж<T> itself (+8 B on every pointer box, corpus-wide).
//   VIEWS_CWT    arm 2 — a ConditionalWeakTable<ж<T>, ViewTable> per T: no instance state, one weak
//                        lookup per call.
//   VIEWS_GATED  arm 3 — the slot only on boxes whose POINTEE TYPE is a consumer: Ꮡ<T>() mints a
//                        SlottedStandardBox<T> when BoxShape<T>.Slotted is set (by the [GoBoxViews]
//                        attribute at type init, or lazily by the first of() on a ж<T>); every other
//                        box, and every box minted before the flip, falls back to the weak table.
//
// The nil box never caches (it is a shared static per T and carries no field to alias). Retention is
// the stated cost that is not per call: a parent box keeps one view per distinct field it has been
// asked for, for its lifetime — the parent → view → parent cycle collects together.
//
// Build: `dotnet build golib.csproj -p:GoViews=slot|cwt|gated` (golib.csproj maps the property to
// the symbol). With no property the file compiles to nothing and of() is byte-for-byte the pre-spike
// path.

#if VIEWS_SLOT || VIEWS_CWT || VIEWS_GATED

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

#if VIEWS_CWT || VIEWS_GATED
/// <summary>The weak-table value: a mutable head for a box that has no slot of its own.</summary>
internal sealed class ViewTable
{
    internal ViewEntry? Head;
}
#endif

#if VIEWS_GATED
/// <summary>
/// Marks a struct type whose boxes carry the field-view slot (arm 3's gate). In a banked design the
/// converter would emit it on every type the census finds a `.of(Ꮡfield)` consumer for; in the spike
/// it is hand-placed, and the lazy flip in <see cref="BoxShape{T}"/> stands in for the rest.
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

/// <summary>A standard box that carries the view slot (arm 3).</summary>
public sealed class SlottedStandardBox<T> : StandardBox<T>
{
    internal ViewEntry? m_views;

    public SlottedStandardBox(in T value) : base(value)
    {
    }
}
#endif

public abstract partial class ж<T>
{
#if VIEWS_SLOT
    // arm 1: the slot on every box
    private ViewEntry? m_views;
#endif

#if VIEWS_CWT || VIEWS_GATED
    private static readonly ConditionalWeakTable<ж<T>, ViewTable> s_views = new();
#endif

    // Where this box's view list lives — the one place the arms differ for a lookup.
    private ref ViewEntry? viewsHead()
    {
#if VIEWS_SLOT
        return ref m_views;
#elif VIEWS_CWT
        return ref s_views.GetOrCreateValue(this).Head;
#else
        if (this is SlottedStandardBox<T> slotted)
            return ref slotted.m_views;

        return ref s_views.GetOrCreateValue(this).Head;
#endif
    }

    /// <summary>
    /// The untyped overload's cached view: minted through <paramref name="fieldRefFunc"/> on the first
    /// request and reused afterwards. The nil box never caches.
    /// </summary>
    private ж<TElem> viewOf<TElem>(FieldRefFunc<TElem> fieldRefFunc)
    {
        if (m_isNull)
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
        if (m_isNull)
            return new FieldRefBox<TElem>(this, FieldRefWrappers<TElem>.For(fieldRefFunc), fieldRefFunc);

        if (tryFindView(fieldRefFunc, out ж<TElem>? found))
            return found;

        return publishView(fieldRefFunc, new FieldRefBox<TElem>(this, FieldRefWrappers<TElem>.For(fieldRefFunc), fieldRefFunc));
    }

    private bool tryFindView<TElem>(Delegate token, out ж<TElem>? view)
    {
#if VIEWS_GATED
        // the lazy flip: the first field view asked of any ж<T> marks T a consumer for every box
        // minted afterwards
        if (!BoxShape<T>.Slotted)
            BoxShape<T>.Slotted = true;
#endif

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
            ViewEntry? seen = head;
            ViewEntry candidate = new(token, view, seen);

            if (ReferenceEquals(Interlocked.CompareExchange(ref head, candidate, seen), seen))
                return view;

            // a racing publisher won: if it cached the same token, share its view
            for (ViewEntry? e = head; e is not null && !ReferenceEquals(e, seen); e = e.Next)
            {
                if (ReferenceEquals(e.Token, token) || e.Token.Equals(token))
                    return (ж<TElem>)e.View;
            }
        }
    }
}

#endif
